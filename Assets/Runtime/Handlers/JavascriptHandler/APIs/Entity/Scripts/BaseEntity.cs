// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a base entity.
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// ID of the entity.
        /// </summary>
        public UUID id
        {
            get
            {
                if (IsValid() == false)
                {
                    Logging.LogError("[BaseEntity:id] Unknown entity.");
                    return null;
                }

                return internalEntity.id == null ? null
                    : new UUID(internalEntity.id.ToString());
            }
        }

        /// <summary>
        /// Tag of the entity.
        /// </summary>
        public string tag
        {
            get
            {
                if (IsValid() == false)
                {
                    Logging.LogError("[BaseEntity:tag] Unknown entity.");
                    return null;
                }
                return internalEntity.entityTag;
            }
            
            set
            {
                if (IsValid() == false)
                {
                    Logging.LogError("[BaseEntity:tag] Unknown entity.");
                }
                else
                {
                    internalEntity.entityTag = value;
                }    
            }
        }

        /// <summary>
        /// Internal entity reference.
        /// </summary>
        internal WorldEngine.Entity.BaseEntity internalEntity = null;

        /// <summary>
        /// Type of the entity.
        /// </summary>
        internal Type internalEntityType = typeof(WorldEngine.Entity.BaseEntity);

        /// <summary>
        /// Constructor for the entity.
        /// </summary>
        public BaseEntity()
        {
            if (WorldEngine.WorldEngine.ActiveWorld == null)
            {
                Logging.LogError("[BaseEntity] Could not find active world.");
                return;
            }
        }

        /// <summary>
        /// Get the entity corresponding to an ID.
        /// </summary>
        /// <param name="id">ID of the entity to get.</param>
        /// <returns>The entity corresponding to the ID, or null.</returns>
        public static BaseEntity Get(Guid id)
        {
            WorldEngine.Entity.BaseEntity internalEntity = WorldEngine.WorldEngine.ActiveWorld.entityManager.FindEntity(id);
            if (internalEntity == null)
            {
                Logging.Log("[BaseEntity->Get] Could not find entity.");
                return null;
            }
            return EntityAPIHelper.GetPublicEntity(internalEntity);
        }

        /// <summary>
        /// Set the parent of the entity.
        /// </summary>
        /// <param name="parent">Entity to make parent of this one, or null.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetParent(BaseEntity parent)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetParent] Unknown entity.");
                return false;
            }

            if (parent == null)
            {
                internalEntity.SetParent(null);
                return true;
            }
            else
            {
                if (parent.IsValid() == false)
                {
                    Logging.LogError("[BaseEntity:SetParent] Unknown parent entity.");
                    return false;
                }

                if (internalEntityType == typeof(WorldEngine.Entity.AirplaneEntity))
                {
                    return ((WorldEngine.Entity.AirplaneEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.AudioEntity))
                {
                    return ((WorldEngine.Entity.AudioEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.AutomobileEntity))
                {
                    return ((WorldEngine.Entity.AutomobileEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.ButtonEntity))
                {
                    return ((WorldEngine.Entity.ButtonEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.CanvasEntity))
                {
                    return ((WorldEngine.Entity.CanvasEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.CharacterEntity))
                {
                    return ((WorldEngine.Entity.CharacterEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.HTMLEntity))
                {
                    return ((WorldEngine.Entity.HTMLEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.HTMLUIElementEntity))
                {
                    return ((WorldEngine.Entity.HTMLUIElementEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.ImageEntity))
                {
                    return ((WorldEngine.Entity.ImageEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.InputEntity))
                {
                    return ((WorldEngine.Entity.InputEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.LightEntity))
                {
                    return ((WorldEngine.Entity.LightEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.MeshEntity))
                {
                    return ((WorldEngine.Entity.MeshEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.TerrainEntity))
                {
                    return ((WorldEngine.Entity.TerrainEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.HybridTerrainEntity))
                {
                    return ((WorldEngine.Entity.HybridTerrainEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.TextEntity))
                {
                    return ((WorldEngine.Entity.TextEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.VoxelEntity))
                {
                    return ((WorldEngine.Entity.VoxelEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.WaterBlockerEntity))
                {
                    return ((WorldEngine.Entity.WaterBlockerEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else if (internalEntityType == typeof(WorldEngine.Entity.WaterBodyEntity))
                {
                    return ((WorldEngine.Entity.WaterBodyEntity) internalEntity).SetParent(EntityAPIHelper.GetPrivateEntity(parent));
                }
                else
                {
                    Logging.LogError("[BaseEntity:SetParent] Unknown entity type.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the parent of the entity.
        /// </summary>
        /// <returns>Entity that is parent of this one, or null if none.</returns>
        public BaseEntity GetParent()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetParent] Unknown entity.");
                return null;
            }

            if (WorldEngine.WorldEngine.ActiveWorld == null)
            {
                Logging.LogError("[BaseEntity:GetParent] Could not find active world.");
                return null;
            }

            if (WorldEngine.WorldEngine.ActiveWorld.entityManager == null)
            {
                Logging.LogError("[BaseEntity:GetParent] Could not find entity manager.");
                return null;
            }

            WorldEngine.Entity.BaseEntity parentEntity = internalEntity.GetParent();
            if (parentEntity == null)
            {
                return null;
            }

            return EntityAPIHelper.GetPublicEntity(parentEntity);
        }

        /// <summary>
        /// Place the world camera on this entity.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool PlaceCameraOn()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:PlaceCameraOn] Unknown entity.");
                return false;
            }

            WorldEngine.WorldEngine.ActiveWorld.cameraManager.SetParent(internalEntity.gameObject);

            return true;
        }

        /// <summary>
        /// Get the children of the entity.
        /// </summary>
        /// <returns>An array of entities that are children of this one.</returns>
        public BaseEntity[] GetChildren()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetChildren] Unknown entity.");
                return null;
            }

            List<BaseEntity> children = new List<BaseEntity>();
            WorldEngine.Entity.BaseEntity[] childEntities = internalEntity.GetChildren();
            foreach (WorldEngine.Entity.BaseEntity childEntity in childEntities)
            {
                BaseEntity child = EntityAPIHelper.GetPublicEntity(childEntity);
                if (child != null)
                {
                    children.Add(child);
                }
            }
            return children.ToArray();
        }

        /// <summary>
        /// Get the transform of the entity.
        /// </summary>
        /// <returns>The transform of the entity.</returns>
        public Transform GetTransform()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetTransform] Unknown entity.");
                return null;
            }

            UnityEngine.Transform transform = internalEntity.transform;
            Transform t = new Transform();
            UnityEngine.Vector3 worldOffset = WorldEngine.WorldEngine.ActiveWorld.worldOffset;
            t.position = new Vector3(transform.position.x - worldOffset.x, transform.position.y - worldOffset.y,
                transform.position.z - worldOffset.z);
            t.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
            t.scale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            return t;
        }

        /// <summary>
        /// Set the position of the entity.
        /// </summary>
        /// <param name="position">Position to apply to the entity.</param>
        /// <param name="local">Whether or not the position is local.</param>
        /// <param name="synchronizeChange">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetPosition(Vector3 position, bool local, bool synchronizeChange = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetPosition] Unknown entity.");
                return false;
            }

            return internalEntity.SetPosition(new UnityEngine.Vector3(position.x, position.y, position.z),
                local, synchronizeChange);
        }

        /// <summary>
        /// Get the position of the entity.
        /// </summary>
        /// <param name="local">Whether or not to get the local position.</param>
        /// <returns>The position of the entity.</returns>
        public Vector3 GetPosition(bool local)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetPosition] Unknown entity.");
                return null;
            }

            UnityEngine.Vector3 position = internalEntity.GetPosition(local);
            return new Vector3(position.x, position.y, position.z);
        }

        /// <summary>
        /// Set the rotation of the entity.
        /// </summary>
        /// <param name="rotation">Rotation to apply to the entity.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        /// <param name="synchronizeChange">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetRotation(Quaternion rotation, bool local, bool synchronizeChange = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetRotation] Unknown entity.");
                return false;
            }

            return internalEntity.SetRotation(new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w),
                local, synchronizeChange);
        }

        /// <summary>
        /// Get the rotation of the entity.
        /// </summary>
        /// <param name="local">Whether or not to get the local rotation.</param>
        /// <returns>The rotation of the entity.</returns>
        public Quaternion GetRotation(bool local)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetRotation] Unknown entity.");
                return null;
            }

            UnityEngine.Quaternion rotation = internalEntity.GetRotation(local);
            return new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        }

        /// <summary>
        /// Set the Euler rotation of the entity.
        /// </summary>
        /// <param name="eulerRotation">Euler rotation to apply to the entity.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        /// <param name="synchronizeChange">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetEulerRotation(Vector3 eulerRotation, bool local, bool synchronizeChange = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetEulerRotation] Unknown entity.");
                return false;
            }

            return internalEntity.SetEulerRotation(new UnityEngine.Vector3(eulerRotation.x, eulerRotation.y, eulerRotation.z),
                local, synchronizeChange);
        }

        /// <summary>
        /// Get the Euler rotation of the entity.
        /// </summary>
        /// <param name="local">Whether or not to get the local rotation.</param>
        /// <returns>The Euler rotation of the entity.</returns>
        public Vector3 GetEulerRotation(bool local)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetEulerRotation] Unknown entity.");
                return null;
            }

            UnityEngine.Vector3 rotation = internalEntity.GetEulerRotation(local);
            return new Vector3(rotation.x, rotation.y, rotation.z);
        }

        /// <summary>
        /// Set the scale of the entity.
        /// </summary>
        /// <param name="scale">Scale to apply to the entity.</param>
        /// <param name="synchronizeChange">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetScale(Vector3 scale, bool synchronizeChange = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetScale] Unknown entity.");
                return false;
            }

            return internalEntity.SetScale(new UnityEngine.Vector3(scale.x, scale.y, scale.z), synchronizeChange);
        }

        /// <summary>
        /// Get the scale of the entity.
        /// </summary>
        /// <returns>The scale of the entity.</returns>
        public Vector3 GetScale()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetScale] Unknown entity.");
                return null;
            }

            UnityEngine.Vector3 scale = internalEntity.GetScale();
            return new Vector3(scale.x, scale.y, scale.z);
        }

        /// <summary>
        /// Set the size of the entity.
        /// </summary>
        /// <param name="size">Size to apply to the entity.</param>
        /// <param name="synchronizeChange">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetSize(Vector3 size, bool synchronizeChange)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetSize] Unknown entity.");
                return false;
            }

            return internalEntity.SetSize(new UnityEngine.Vector3(size.x, size.y, size.z), synchronizeChange);
        }

        /// <summary>
        /// Get the size of the entity.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public Vector3 GetSize()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetSize] Unknown entity.");
                return null;
            }

            UnityEngine.Vector3 size = internalEntity.GetSize();
            return new Vector3(size.x, size.y, size.z);
        }

        /// <summary>
        /// Set the visibility of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to make entity visible.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public virtual bool SetVisibility(bool visible, bool synchronize = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetVisibility] Unknown entity.");
                return false;
            }

            return internalEntity.SetVisibility(visible, synchronize);
        }

        /// <summary>
        /// Get the visibility of the entity.
        /// </summary>
        /// <returns>The visibility of the entity.</returns>
        public bool GetVisibility()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetVisibility] Unknown entity.");
                return false;
            }

            return internalEntity.GetVisibility();
        }

        /// <summary>
        /// Set the highlight state of the entity.
        /// </summary>
        /// <param name="highlight">Whether or not to highlight the entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetHighlight(bool highlight)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetHighlight] Unknown entity.");
                return false;
            }

            internalEntity.SetHighlight(highlight);
            return true;
        }

        /// <summary>
        /// Get the highlight state of the entity.
        /// </summary>
        /// <returns>The highlight state of the entity.</returns>
        public bool GetHighlight()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetHighlight] Unknown entity.");
                return false;
            }
            
            return internalEntity.GetHighlight();
        }

        /// <summary>
        /// Delete the entity.
        /// </summary>
        /// <param name="synchronizeChange">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Delete(bool synchronizeChange = true)
        {
            if (IsValid() == false)
            {
                Logging.LogWarning("[BaseEntity:Delete] Unknown entity.");
                return false;
            }

            return internalEntity.Delete(synchronizeChange);
        }

        /// <summary>
        /// Compare this entity with another.
        /// </summary>
        /// <returns>Whether or not the entities match.</returns>
        public bool Compare(BaseEntity other)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:Compare] Unknown entity.");
                return false;
            }

            WorldEngine.Entity.BaseEntity otherEntity = EntityAPIHelper.GetPrivateEntity(other);
            return otherEntity == internalEntity;
        }

        /// <summary>
        /// Set the motion state for this entity.
        /// </summary>
        /// <param name="motionToSet">Motion state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetMotion(EntityMotion motion)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetMotion] Unknown entity.");
                return false;
            }

            WorldEngine.Entity.BaseEntity.EntityMotion eMotion = new WorldEngine.Entity.BaseEntity.EntityMotion()
            {
                angularVelocity = new UnityEngine.Vector3(motion.angularVelocity.x, motion.angularVelocity.y, motion.angularVelocity.z),
                stationary = motion.stationary,
                velocity = new UnityEngine.Vector3(motion.velocity.x, motion.velocity.y, motion.velocity.z)
            };

            if (internalEntityType == typeof(WorldEngine.Entity.AirplaneEntity))
            {
                return ((WorldEngine.Entity.AirplaneEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AudioEntity))
            {
                return ((WorldEngine.Entity.AudioEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AutomobileEntity))
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ButtonEntity))
            {
                return ((WorldEngine.Entity.ButtonEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CanvasEntity))
            {
                return ((WorldEngine.Entity.CanvasEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CharacterEntity))
            {
                return ((WorldEngine.Entity.CharacterEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLEntity))
            {
                return ((WorldEngine.Entity.HTMLEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLUIElementEntity))
            {
                return ((WorldEngine.Entity.HTMLUIElementEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ImageEntity))
            {
                return ((WorldEngine.Entity.ImageEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.InputEntity))
            {
                return ((WorldEngine.Entity.InputEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.LightEntity))
            {
                return ((WorldEngine.Entity.LightEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.MeshEntity))
            {
                return ((WorldEngine.Entity.MeshEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TerrainEntity))
            {
                return ((WorldEngine.Entity.TerrainEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HybridTerrainEntity))
            {
                return ((WorldEngine.Entity.HybridTerrainEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TextEntity))
            {
                return ((WorldEngine.Entity.TextEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.VoxelEntity))
            {
                return ((WorldEngine.Entity.VoxelEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBlockerEntity))
            {
                return ((WorldEngine.Entity.WaterBlockerEntity) internalEntity).SetMotion(eMotion);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBodyEntity))
            {
                return ((WorldEngine.Entity.WaterBodyEntity) internalEntity).SetMotion(eMotion);
            }
            else
            {
                Logging.LogError("[BaseEntity:SetMotion] Unknown entity type.");
                return false;
            }
        }

        /// <summary>
        /// Get the motion state for this entity.
        /// </summary>
        /// <returns>The motion state for this entity.</returns>
        public virtual EntityMotion GetMotion()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetMotion] Unknown entity.");
                return new EntityMotion { angularVelocity = Vector3.zero, stationary = true, velocity = Vector3.zero };
            }

            WorldEngine.Entity.BaseEntity.EntityMotion? eMotion = null;
            if (internalEntityType == typeof(WorldEngine.Entity.AirplaneEntity))
            {
                eMotion = ((WorldEngine.Entity.AirplaneEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AudioEntity))
            {
                eMotion = ((WorldEngine.Entity.AudioEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AutomobileEntity))
            {
                eMotion = ((WorldEngine.Entity.AutomobileEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ButtonEntity))
            {
                eMotion = ((WorldEngine.Entity.ButtonEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CanvasEntity))
            {
                eMotion = ((WorldEngine.Entity.CanvasEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CharacterEntity))
            {
                eMotion = ((WorldEngine.Entity.CharacterEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLEntity))
            {
                eMotion = ((WorldEngine.Entity.HTMLEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLUIElementEntity))
            {
                eMotion = ((WorldEngine.Entity.HTMLUIElementEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ImageEntity))
            {
                eMotion = ((WorldEngine.Entity.ImageEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.InputEntity))
            {
                eMotion = ((WorldEngine.Entity.InputEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.LightEntity))
            {
                eMotion = ((WorldEngine.Entity.LightEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.MeshEntity))
            {
                eMotion = ((WorldEngine.Entity.MeshEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TerrainEntity))
            {
                eMotion = ((WorldEngine.Entity.TerrainEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HybridTerrainEntity))
            {
                eMotion = ((WorldEngine.Entity.HybridTerrainEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TextEntity))
            {
                eMotion = ((WorldEngine.Entity.TextEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.VoxelEntity))
            {
                eMotion = ((WorldEngine.Entity.VoxelEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBlockerEntity))
            {
                eMotion = ((WorldEngine.Entity.WaterBlockerEntity) internalEntity).GetMotion();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBodyEntity))
            {
                eMotion = ((WorldEngine.Entity.WaterBodyEntity) internalEntity).GetMotion();
            }
            else
            {
                Logging.LogError("[BaseEntity:GetMotion] Unknown entity type.");
                return new EntityMotion()
                {
                    angularVelocity = Vector3.zero,
                    stationary = false,
                    velocity = Vector3.zero
                };
            }

            if (eMotion.HasValue == false)
            {
                return new EntityMotion()
                {
                    angularVelocity = Vector3.zero,
                    stationary = false,
                    velocity = Vector3.zero
                };
            }
            else
            {
                return new EntityMotion()
                {
                    angularVelocity = new Vector3(eMotion.Value.angularVelocity.x,
                        eMotion.Value.angularVelocity.y, eMotion.Value.angularVelocity.z),
                    stationary = eMotion.Value.stationary.HasValue ? eMotion.Value.stationary.Value : true,
                    velocity = new Vector3(eMotion.Value.velocity.x, eMotion.Value.velocity.y, eMotion.Value.velocity.z)
                };
            }
        }

        /// <summary>
        /// Set the physical properties of the entity.
        /// </summary>
        /// <param name="propertiesToSet">Properties to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetPhysicalProperties(EntityPhysicalProperties properties)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetPhysicalProperties] Unknown entity.");
                return false;
            }
            WorldEngine.Entity.BaseEntity.EntityPhysicalProperties props = new WorldEngine.Entity.BaseEntity.EntityPhysicalProperties()
            {
                angularDrag = properties.angularDrag,
                centerOfMass = properties.centerOfMass is null ? null :
                    new UnityEngine.Vector3(properties.centerOfMass.x, properties.centerOfMass.y, properties.centerOfMass.z),
                drag = properties.drag,
                gravitational = properties.gravitational,
                mass = properties.mass
            };

            if (internalEntityType == typeof(WorldEngine.Entity.AirplaneEntity))
            {
                return ((WorldEngine.Entity.AirplaneEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AudioEntity))
            {
                return ((WorldEngine.Entity.AudioEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AutomobileEntity))
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ButtonEntity))
            {
                return ((WorldEngine.Entity.ButtonEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CanvasEntity))
            {
                return ((WorldEngine.Entity.CanvasEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CharacterEntity))
            {
                return ((WorldEngine.Entity.CharacterEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLEntity))
            {
                return ((WorldEngine.Entity.HTMLEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLUIElementEntity))
            {
                return ((WorldEngine.Entity.HTMLUIElementEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ImageEntity))
            {
                return ((WorldEngine.Entity.ImageEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.InputEntity))
            {
                return ((WorldEngine.Entity.InputEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.LightEntity))
            {
                return ((WorldEngine.Entity.LightEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.MeshEntity))
            {
                return ((WorldEngine.Entity.MeshEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TerrainEntity))
            {
                return ((WorldEngine.Entity.TerrainEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HybridTerrainEntity))
            {
                return ((WorldEngine.Entity.HybridTerrainEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TextEntity))
            {
                return ((WorldEngine.Entity.TextEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.VoxelEntity))
            {
                return ((WorldEngine.Entity.VoxelEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBlockerEntity))
            {
                return ((WorldEngine.Entity.WaterBlockerEntity) internalEntity).SetPhysicalProperties(props);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBodyEntity))
            {
                return ((WorldEngine.Entity.WaterBodyEntity) internalEntity).SetPhysicalProperties(props);
            }
            else
            {
                Logging.LogError("[BaseEntity:SetPhysicalProperties] Unknown entity type.");
                return false;
            }
        }

        /// <summary>
        /// Get the physical properties for the entity.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public virtual EntityPhysicalProperties GetPhysicalProperties()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetMotion] Unknown entity.");
                return new EntityPhysicalProperties(0, Vector3.zero, 0, false, 0);
            }

            WorldEngine.Entity.BaseEntity.EntityPhysicalProperties? props = null;
            if (internalEntityType == typeof(WorldEngine.Entity.AirplaneEntity))
            {
                props = ((WorldEngine.Entity.AirplaneEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AudioEntity))
            {
                props = ((WorldEngine.Entity.AudioEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AutomobileEntity))
            {
                props = ((WorldEngine.Entity.AutomobileEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ButtonEntity))
            {
                props = ((WorldEngine.Entity.ButtonEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CanvasEntity))
            {
                props = ((WorldEngine.Entity.CanvasEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CharacterEntity))
            {
                props = ((WorldEngine.Entity.CharacterEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLEntity))
            {
                props = ((WorldEngine.Entity.HTMLEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLUIElementEntity))
            {
                props = ((WorldEngine.Entity.HTMLUIElementEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ImageEntity))
            {
                props = ((WorldEngine.Entity.ImageEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.InputEntity))
            {
                props = ((WorldEngine.Entity.InputEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.LightEntity))
            {
                props = ((WorldEngine.Entity.LightEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.MeshEntity))
            {
                props = ((WorldEngine.Entity.MeshEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TerrainEntity))
            {
                props = ((WorldEngine.Entity.TerrainEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HybridTerrainEntity))
            {
                props = ((WorldEngine.Entity.HybridTerrainEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TextEntity))
            {
                props = ((WorldEngine.Entity.TextEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.VoxelEntity))
            {
                props = ((WorldEngine.Entity.VoxelEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBlockerEntity))
            {
                props = ((WorldEngine.Entity.WaterBlockerEntity) internalEntity).GetPhysicalProperties();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBodyEntity))
            {
                props = ((WorldEngine.Entity.WaterBodyEntity) internalEntity).GetPhysicalProperties();
            }
            else
            {
                Logging.LogError("[BaseEntity:GetPhysicalProperties] Unknown entity type.");
                return new EntityPhysicalProperties(0, Vector3.zero, 0, false, 0);
            }

            if (props.HasValue == false)
            {
                return new EntityPhysicalProperties(0, Vector3.zero, 0, false, 0);
            }
            else
            {
                return new EntityPhysicalProperties(props.Value.angularDrag.HasValue ? props.Value.angularDrag.Value : 0,
                    props.Value.centerOfMass == null ? null :
                        new Vector3(props.Value.centerOfMass.Value.x, props.Value.centerOfMass.Value.y, props.Value.centerOfMass.Value.z),
                    props.Value.drag.HasValue ? props.Value.drag.Value : 0,
                    props.Value.gravitational.HasValue ? props.Value.gravitational.Value : false,
                    props.Value.mass.HasValue ? props.Value.mass.Value : 0);
            }
        }

        /// <summary>
        /// Set the interaction state for the entity.
        /// </summary>
        /// <param name="stateToSet">Interaction state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetInteractionState(InteractionState state)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetInteractionState] Unknown entity.");
                return false;
            }

            WorldEngine.Entity.BaseEntity.InteractionState iState = WorldEngine.Entity.BaseEntity.InteractionState.Hidden;
            switch (state)
            {
                case InteractionState.Hidden:
                    iState = WorldEngine.Entity.BaseEntity.InteractionState.Hidden;
                    break;

                case InteractionState.Static:
                    iState = WorldEngine.Entity.BaseEntity.InteractionState.Static;
                    break;

                case InteractionState.Placing:
                    iState = WorldEngine.Entity.BaseEntity.InteractionState.Placing;
                    break;

                case InteractionState.Physical:
                    iState = WorldEngine.Entity.BaseEntity.InteractionState.Physical;
                    break;

            }

            if (internalEntityType == typeof(WorldEngine.Entity.AirplaneEntity))
            {
                return ((WorldEngine.Entity.AirplaneEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AudioEntity))
            {
                return ((WorldEngine.Entity.AudioEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AutomobileEntity))
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ButtonEntity))
            {
                return ((WorldEngine.Entity.ButtonEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CanvasEntity))
            {
                return ((WorldEngine.Entity.CanvasEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CharacterEntity))
            {
                return ((WorldEngine.Entity.CharacterEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLEntity))
            {
                return ((WorldEngine.Entity.HTMLEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLUIElementEntity))
            {
                return ((WorldEngine.Entity.HTMLUIElementEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ImageEntity))
            {
                return ((WorldEngine.Entity.ImageEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.InputEntity))
            {
                return ((WorldEngine.Entity.InputEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.LightEntity))
            {
                return ((WorldEngine.Entity.LightEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.MeshEntity))
            {
                return ((WorldEngine.Entity.MeshEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TerrainEntity))
            {
                return ((WorldEngine.Entity.TerrainEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HybridTerrainEntity))
            {
                return ((WorldEngine.Entity.HybridTerrainEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TextEntity))
            {
                return ((WorldEngine.Entity.TextEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.VoxelEntity))
            {
                return ((WorldEngine.Entity.VoxelEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBlockerEntity))
            {
                return ((WorldEngine.Entity.WaterBlockerEntity) internalEntity).SetInteractionState(iState);
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBodyEntity))
            {
                return ((WorldEngine.Entity.WaterBodyEntity) internalEntity).SetInteractionState(iState);
            }
            else
            {
                Logging.LogError("[BaseEntity:SetInteractionState] Unknown entity type.");
                return false;
            }
        }

        /// <summary>
        /// Get the interaction state of the entity.
        /// </summary>
        /// <returns>The interaction state for this entity.</returns>
        public InteractionState GetInteractionState()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetInteractionState] Unknown entity.");
                return InteractionState.Hidden;
            }

            WorldEngine.Entity.BaseEntity.InteractionState iState
                = WorldEngine.Entity.BaseEntity.InteractionState.Hidden;
            if (internalEntityType == typeof(WorldEngine.Entity.AirplaneEntity))
            {
                iState = ((WorldEngine.Entity.AirplaneEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AudioEntity))
            {
                iState = ((WorldEngine.Entity.AudioEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.AutomobileEntity))
            {
                iState = ((WorldEngine.Entity.AutomobileEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ButtonEntity))
            {
               iState = ((WorldEngine.Entity.ButtonEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CanvasEntity))
            {
                iState = ((WorldEngine.Entity.CanvasEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.CharacterEntity))
            {
                iState = ((WorldEngine.Entity.CharacterEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLEntity))
            {
                iState = ((WorldEngine.Entity.HTMLEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HTMLUIElementEntity))
            {
                iState = ((WorldEngine.Entity.HTMLUIElementEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.ImageEntity))
            {
                iState = ((WorldEngine.Entity.ImageEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.InputEntity))
            {
                iState = ((WorldEngine.Entity.InputEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.LightEntity))
            {
                iState = ((WorldEngine.Entity.LightEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.MeshEntity))
            {
                iState = ((WorldEngine.Entity.MeshEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TerrainEntity))
            {
                iState = ((WorldEngine.Entity.TerrainEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.HybridTerrainEntity))
            {
                iState = ((WorldEngine.Entity.HybridTerrainEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.TextEntity))
            {
                iState = ((WorldEngine.Entity.TextEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.VoxelEntity))
            {
                iState = ((WorldEngine.Entity.VoxelEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBlockerEntity))
            {
                iState = ((WorldEngine.Entity.WaterBodyEntity) internalEntity).GetInteractionState();
            }
            else if (internalEntityType == typeof(WorldEngine.Entity.WaterBlockerEntity))
            {
                iState = ((WorldEngine.Entity.WaterBodyEntity) internalEntity).GetInteractionState();
            }
            else
            {
                Logging.LogError("[BaseEntity:GetInteractionState] Unknown entity type.");
                return InteractionState.Hidden;
            }

            switch (iState)
            {
                case WorldEngine.Entity.BaseEntity.InteractionState.Hidden:
                    return InteractionState.Hidden;

                case WorldEngine.Entity.BaseEntity.InteractionState.Static:
                    return InteractionState.Static;

                case WorldEngine.Entity.BaseEntity.InteractionState.Placing:
                    return InteractionState.Placing;

                case WorldEngine.Entity.BaseEntity.InteractionState.Physical:
                    return InteractionState.Physical;

                default:
                    return InteractionState.Hidden;
            }
        }

        /// <summary>
        /// Get a raycast forward from this entity.
        /// </summary>
        /// <returns>A raycast forward from this entity, or null.</returns>
        public RaycastHitInfo? GetRaycast()
        {
            return GetRaycast(Vector3.forward);
        }

        /// <summary>
        /// Get a raycast from this entity.
        /// </summary>
        /// <param name="direction">Direction to cast the ray in.</param>
        /// <returns>A raycast from this entity, or null.</returns>
        public RaycastHitInfo? GetRaycast(Vector3 direction)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:GetRaycast] Unknown entity.");
                return null;
            }

            UnityEngine.RaycastHit hit;
            if (UnityEngine.Physics.Raycast(internalEntity.transform.position,
                new UnityEngine.Vector3(direction.x, direction.y, direction.z), out hit))
            {
                WorldEngine.Entity.BaseEntity hitEntity = null;
                if (hitEntity = hit.collider.GetComponentInParent<WorldEngine.Entity.BaseEntity>())
                {
                    BaseEntity hitPublicEntity = EntityAPIHelper.GetPublicEntity(hitEntity);
                    if (hitPublicEntity == null)
                    {
                        return null;
                    }

                    UnityEngine.Vector3 worldOffset = WebVerse.WorldEngine.WorldEngine.ActiveWorld.worldOffset;
                    return new RaycastHitInfo()
                    {
                        entity = hitPublicEntity,
                        hitPoint = new Vector3(hit.point.x - worldOffset.x,
                            hit.point.y - worldOffset.y, hit.point.z - worldOffset.z),
                        hitPointNormal = new Vector3(hit.normal.x, hit.normal.y, hit.normal.z),
                        origin = GetPosition(false)
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Add a placement socket to the entity.
        /// </summary>
        /// <param name="position">Position of the placement socket relative to the entity.</param>
        /// <param name="rotation">Rotation of the placement socket relative to the entity.</param>
        /// <param name="connectingOffset">Offset to apply when connecting to another socket.</param>
        public virtual bool AddSocket(Vector3 position, Quaternion rotation, Vector3 connectingOffset)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:AddSocket] Unknown entity.");
                return false;
            }

            if (internalEntityType != typeof(WorldEngine.Entity.MeshEntity))
            {
                Logging.LogWarning("[BaseEntity:AddSocket] Sockets only supported on mesh entities.");
                return false;
            }

            internalEntity.AddSocket(new UnityEngine.Vector3(position.x, position.y, position.z),
                new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w),
                new UnityEngine.Vector3(connectingOffset.x, connectingOffset.y, connectingOffset.z));

            return true;
        }

        /// <summary>
        /// Play an animation.
        /// </summary>
        /// <param name="animationName">Name of animation to play.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public virtual bool PlayAnimation(string animationName)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:PlayAnimation] Unknown entity.");
                return false;
            }

            return internalEntity.PlayAnimation(animationName);
        }

        /// <summary>
        /// Stop an animation.
        /// </summary>
        /// <param name="animationName">Name of animation to stop.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public virtual bool StopAnimation(string animationName)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:StopAnimation] Unknown entity.");
                return false;
            }

            return internalEntity.StopAnimation(animationName);
        }

        /// <summary>
        /// Set the speed of an animation.
        /// </summary>
        /// <param name="animationName">Name of animation to set speed of.</param>
        /// <param name="speed">Speed to set animation to.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public virtual bool SetAnimationSpeed(string animationName, float speed)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:SetAnimationSpeed] Unknown entity.");
                return false;
            }

            return internalEntity.SetAnimationSpeed(animationName, speed);
        }

        /// <summary>
        /// Enable broadcasting of position via synchronizer.
        /// </summary>
        /// <param name="interval">Interval at which to broadcast, <= 0 to not broadcast.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public virtual bool EnablePositionBroadcast(float interval)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:EnablePositionBroadcast] Unknown entity.");
                return false;
            }

            return internalEntity.EnablePositionBroadcast(interval);
        }

        /// <summary>
        /// Disable broadcasting of position via synchronizer.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public virtual bool DisablePositionBroadcast()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:DisablePositionBroadcast] Unknown entity.");
                return false;
            }

            return internalEntity.DisablePositionBroadcast();
        }

        /// <summary>
        /// Enable broadcasting of rotation via synchronizer.
        /// </summary>
        /// <param name="interval">Interval at which to broadcast, <= 0 to not broadcast.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public virtual bool EnableRotationBroadcast(float interval)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:EnableRotationBroadcast] Unknown entity.");
                return false;
            }

            return internalEntity.EnableRotationBroadcast(interval);
        }

        /// <summary>
        /// Disable broadcasting of rotation via synchronizer.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public virtual bool DisableRotationBroadcast()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[BaseEntity:DisableRotationBroadcast] Unknown entity.");
                return false;
            }

            return internalEntity.DisableRotationBroadcast();
        }

        /// <summary>
        /// Determines whether or not this entity is valid.
        /// </summary>
        /// <returns>Whether or not this entity is valid.</returns>
        protected bool IsValid()
        {
            if (internalEntity == null)
            {
                return false;
            }

            return true;
        }
    }
}