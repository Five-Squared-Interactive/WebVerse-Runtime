// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a generic entity.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Get the entity corresponding to an ID.
        /// </summary>
        /// <param name="id">ID of the entity to get.</param>
        /// <returns>The entity corresponding to the ID, or null.</returns>
        public static object Get(string id)
        {
            if (id == null)
            {
                Logging.LogWarning("[Entity:Get] Invalid id.");
                return null;
            }
            
            BaseEntity result = BaseEntity.Get(System.Guid.Parse(id));

            if (result == null)
            {
                return null;
            }
            else if (result is AudioEntity)
            {
                return (AudioEntity) result;
            }
            else if (result is ButtonEntity)
            {
                return (ButtonEntity) result;
            }
            else if (result is CanvasEntity)
            {
                return (CanvasEntity) result;
            }
            else if (result is CharacterEntity)
            {
                return (CharacterEntity) result;
            }
            else if (result is ContainerEntity)
            {
                return (ContainerEntity) result;
            }
            else if (result is HTMLEntity)
            {
                return ((HTMLEntity) result);
            }
            else if (result is ImageEntity)
            {
                return ((ImageEntity) result);
            }
            else if (result is InputEntity)
            {
                return (InputEntity) result;
            }
            else if (result is LightEntity)
            {
                return (LightEntity) result;
            }
            else if (result is MeshEntity)
            {
                return (MeshEntity) result;
            }
            else if (result is TerrainEntity)
            {
                return (TerrainEntity) result;
            }
            else if (result is TextEntity)
            {
                return (TextEntity) result;
            }
            else if (result is VoxelEntity)
            {
                return (VoxelEntity) result;
            }
            else if (result is WaterBlockerEntity)
            {
                return (WaterBlockerEntity) result;
            }
            else if (result is WaterEntity)
            {
                return (WaterEntity) result;
            }
            else
            {
                Logging.LogError("[Entity:Get] Unknown entity type.");
                return null;
            }
        }

        /// <summary>
        /// Get the entity corresponding to a tag.
        /// </summary>
        /// <param name="tag">Tag of the entity to get.</param>
        /// <returns>The entity corresponding to the tag, or null.</returns>
        public static object GetByTag(string tag)
        {
            foreach (StraightFour.Entity.BaseEntity entity in StraightFour.StraightFour.ActiveWorld.entityManager.GetAllEntities())
            {
                if (entity.entityTag == tag)
                {
                    if (entity.id == null)
                    {
                        Logging.LogError("[Entity:GetByTag] Entity has no ID.");
                        return null;
                    }

                    return Get(entity.id.ToString());
                }
            }

            return null;
        }
    }
}