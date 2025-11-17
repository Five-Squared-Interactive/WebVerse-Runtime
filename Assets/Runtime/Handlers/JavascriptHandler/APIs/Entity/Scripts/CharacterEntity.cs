// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a character entity.
    /// </summary>
    public class CharacterEntity : BaseEntity
    {
        /// <summary>
        /// Create a character entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="scale">Scale of the entity relative to its parent.</param>
        /// <param name="isSize">Whether or not the scale parameter is a size.</param>
        /// <param name="tag">Tag of the character entity.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// character entity object.</param>
        /// <returns>The character entity object.</returns>
        public static CharacterEntity Create(BaseEntity parent, Vector3 position, Quaternion rotation, Vector3 scale,
            bool isSize = false, string tag = null, string id = null, string onLoaded = null)
        {
            return Create(parent, null, null, Vector3.zero, Quaternion.identity, Vector3.zero,
                position, rotation, scale, isSize, tag, id, onLoaded);
        }

        /// <summary>
        /// Create a character entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="meshObject">Path to the mesh object to load for this entity.</param>
        /// <param name="meshResources">Paths to mesh resources for this entity.</param>
        /// <param name="meshOffset">Offset for the mesh character entity object.</param>
        /// <param name="meshRotation">Rotation for the mesh character entity object.</param>
        /// <param name="avatarLabelOffset">Offset for the avatar label.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="scale">Scale of the entity relative to its parent.</param>
        /// <param name="isSize">Whether or not the scale parameter is a size.</param>
        /// <param name="tag">Tag of the character entity.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// character entity object.</param>
        /// <returns>The character entity object.</returns>
        public static CharacterEntity Create(BaseEntity parent, string meshObject, string[] meshResources,
            Vector3 meshOffset, Quaternion meshRotation, Vector3 avatarLabelOffset, Vector3 position,
            Quaternion rotation, Vector3 scale, bool isSize = false, string tag = null, string id = null,
            string onLoaded = null)
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
            UnityEngine.Vector3 mo = new UnityEngine.Vector3(meshOffset.x, meshOffset.y, meshOffset.z);
            UnityEngine.Quaternion mr = new UnityEngine.Quaternion(meshRotation.x, meshRotation.y, meshRotation.z, meshRotation.w);
            UnityEngine.Vector3 alo = new UnityEngine.Vector3(avatarLabelOffset.x, avatarLabelOffset.y, avatarLabelOffset.z);
            UnityEngine.Vector3 pos = new UnityEngine.Vector3(position.x, position.y, position.z);
            UnityEngine.Quaternion rot = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
            UnityEngine.Vector3 scl = new UnityEngine.Vector3(scale.x, scale.y, scale.z);

            CharacterEntity ce = new CharacterEntity();

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                ce.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(ce.internalEntity, ce);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ce });
                }
            };

            System.Action<StraightFour.Entity.CharacterEntity> onEntityLoadedAction =
                new System.Action<StraightFour.Entity.CharacterEntity>((characterEntity) =>
                {
                    if (characterEntity == null)
                    {
                        Logging.LogError("[MeshEntity:Create] Error loading character entity.");
                    }
                    else
                    {
                        characterEntity.SetParent(pBE);
                        characterEntity.SetPosition(pos, true);
                        characterEntity.SetRotation(rot, true);
                        characterEntity.entityTag = tag;

                        ce.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                        EntityAPIHelper.AddEntityMapping(ce.internalEntity, ce);
                        if (!string.IsNullOrEmpty(onLoaded))
                        {
                            WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ce });
                        }
                    }

                });

            if (string.IsNullOrEmpty(meshObject))
            {
                StraightFour.StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(pBE, null, UnityEngine.Vector3.zero,
                    UnityEngine.Quaternion.identity, UnityEngine.Vector3.zero,
                    pos, rot, scl, guid, tag, isSize, onLoadAction);
            }
            else
            {
                WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsCharacterEntity(
                    meshObject, meshResources, mo, mr, alo, guid, onEntityLoadedAction);
            }

            return ce;
        }

        /// <summary>
        /// Create a character entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the character entity configuration.</param>
        /// <param name="parent">Parent entity for the character entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created character entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, characterEntity) =>
            {
                if (!success || characterEntity == null || !(characterEntity is StraightFour.Entity.CharacterEntity))
                {
                    Logging.LogError("[CharacterEntity:Create] Error loading character entity from JSON.");
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
                                (StraightFour.Entity.CharacterEntity) characterEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadCharacterEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal CharacterEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.CharacterEntity);
        }

        /// <summary>
        /// Whether or not to fix the height if below ground.
        /// </summary>
        public bool fixHeight
        {
            get
            {
                if (IsValid() == false)
                {
                    Logging.LogError("[CharacterEntity:fixHeight] Unknown entity.");
                    return false;
                }

                return ((StraightFour.Entity.CharacterEntity) internalEntity).fixHeight;
            }

            set
            {
                if (IsValid() == false)
                {
                    Logging.LogError("[CharacterEntity:fixHeight] Unknown entity.");
                    return;
                }

                ((StraightFour.Entity.CharacterEntity) internalEntity).fixHeight = value;
            }
        }

        /// <summary>
        /// Set the character model.
        /// </summary>
        /// <param name="newCharacterGO">The new character model to use.</param>
        /// <param name="meshOffset">The offset to apply.</param>
        /// <param name="meshRotation">The rotation to apply.</param>
        /// <param name="newOffset">The offset to apply to the label.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetCharacterModel(string meshObject, Vector3 meshOffset, Quaternion meshRotation,
            Vector3 labelOffset)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CharacterEntity:SetCharacterModel] Unknown entity.");
                return false;
            }

            return WebVerseRuntime.Instance.gltfHandler.ApplyGLTFResourceToCharacterEntity(
                (StraightFour.Entity.CharacterEntity) internalEntity, meshObject, new string[] { meshObject },
                new UnityEngine.Vector3(meshOffset.x, meshOffset.y, meshOffset.z),
                new UnityEngine.Quaternion(meshRotation.x, meshRotation.y, meshRotation.z, meshRotation.w),
                new UnityEngine.Vector3(labelOffset.x, labelOffset.y, labelOffset.z));
        }

        /// <summary>
        /// Set the character model offset.
        /// </summary>
        /// <param name="newOffset">The new offset to apply.</param>
        /// <param name="synchronize">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetCharacterModelOffset(Vector3 newOffset, bool synchronize = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CharacterEntity:SetCharacterModelOffset] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CharacterEntity) internalEntity).SetCharacterObjectOffset(
                new UnityEngine.Vector3(newOffset.x, newOffset.y, newOffset.z), synchronize);
        }

        /// <summary>
        /// Set the character model rotation.
        /// </summary>
        /// <param name="newRotation">The new rotation to apply.</param>
        /// <param name="synchronize">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetCharacterModelRotation(Quaternion newRotation, bool synchronize = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CharacterEntity:SetCharacterModelRotation] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CharacterEntity) internalEntity).SetCharacterObjectRotation(
                new UnityEngine.Quaternion(newRotation.x, newRotation.y, newRotation.z, newRotation.w), synchronize);
        }

        /// <summary>
        /// Set the character label offset.
        /// </summary>
        /// <param name="newOffset">The new offset to apply.</param>
        /// <param name="synchronize">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetCharacterLabelOffset(Vector3 newOffset, bool synchronize = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CharacterEntity:SetCharacterLabelOffset] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CharacterEntity) internalEntity).SetCharacterLabelOffset(
                new UnityEngine.Vector3(newOffset.x, newOffset.y, newOffset.z), synchronize);
        }

        /// <summary>
        /// Apply motion to the character entity with the given vector.
        /// </summary>
        /// <param name="amount">Amount to move the character entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Move(Vector3 amount)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CharacterEntity:Move] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CharacterEntity) internalEntity).Move(
                new UnityEngine.Vector3(amount.x, amount.y, amount.z));
        }

        /// <summary>
        /// Apply a jump to the character entity by the given amount.
        /// </summary>
        /// <param name="amount">Amount to jump the character entity.</param>
        /// <param name="discardIfFalling">Whether or not to discard jump if currently falling.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Jump(float amount)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CharacterEntity:Jump] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CharacterEntity) internalEntity).Jump(amount);
        }

        /// <summary>
        /// Returns whether or not the character entity is on a surface.
        /// </summary>
        /// <returns>Whether or not the character entity is on a surface.</returns>
        public bool IsOnSurface()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CharacterEntity:IsOnSurface] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CharacterEntity) internalEntity).IsOnSurface();
        }

        /// <summary>
        /// Set the visibility of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to make entity visible.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override bool SetVisibility(bool visible, bool synchronize = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CharacterEntity:IsOnSurface] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CharacterEntity) internalEntity).SetVisibility(visible, synchronize);
        }
    }
}