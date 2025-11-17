// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for an input entity.
    /// </summary>
    public class InputEntity : BaseEntity
    {
        /// <summary>
        /// Create an input entity.
        /// </summary>
        /// <param name="parent">Parent canvas of the entity to create.</param>
        /// <param name="positionPercent">Position of the entity within its canvas.</param>
        /// <param name="sizePercent">Size of the entity relative to its canvas.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// input entity object.</param>
        /// <returns>The ID of the input entity object.</returns>
        public static InputEntity Create(CanvasEntity parent,
            Vector2 positionPercent, Vector2 sizePercent,
            string id = null, string tag = null, string onLoaded = null)
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

            StraightFour.Entity.CanvasEntity pCE = (StraightFour.Entity.CanvasEntity) EntityAPIHelper.GetPrivateEntity(parent);
            if (pCE == null)
            {
                Logging.LogWarning("[InputEntity->Create] Invalid parent entity.");
                return null;
            }

            UnityEngine.Vector2 pos = new UnityEngine.Vector2(positionPercent.x, positionPercent.y);
            UnityEngine.Vector2 size = new UnityEngine.Vector2(sizePercent.x, sizePercent.y);

            InputEntity ie = new InputEntity();

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                ie.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(ie.internalEntity, ie);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ie });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadInputEntity(pCE, pos, size, guid, tag, onLoadAction);

            return ie;
        }

        /// <summary>
        /// Create an input entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the input entity configuration.</param>
        /// <param name="parent">Parent entity for the input entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created input entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, inputEntity) =>
            {
                if (!success || inputEntity == null || !(inputEntity is StraightFour.Entity.InputEntity))
                {
                    Logging.LogError("[InputEntity:Create] Error loading input entity from JSON.");
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
                                (StraightFour.Entity.InputEntity) inputEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadInputEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal InputEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.InputEntity);
        }

        /// <summary>
        /// Get the text for the input entity.
        /// </summary>
        /// <returns>Text of the input entity.</returns>
        public string GetText()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[InputEntity:GetText] Unknown entity.");
                return null;
            }

            return ((StraightFour.Entity.InputEntity) internalEntity).GetText();
        }

        /// <summary>
        /// Set the text for the input entity.
        /// </summary>
        /// <param name="text">Text for the input entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetText(string text)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[InputEntity:SetText] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.InputEntity) internalEntity).SetText(text);
        }
    }
}