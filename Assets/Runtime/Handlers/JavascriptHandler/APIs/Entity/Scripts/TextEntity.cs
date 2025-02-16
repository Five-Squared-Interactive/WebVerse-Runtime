// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a text entity.
    /// </summary>
    public class TextEntity : BaseEntity
    {
        /// <summary>
        /// Create a text entity.
        /// </summary>
        /// <param name="parent">Parent canvas of the entity to create.</param>
        /// <param name="text">Text to apply to the new text entity.</param>
        /// <param name="fontSize">Font size to apply to the new text entity.</param>
        /// <param name="positionPercent">Position of the entity within its canvas.</param>
        /// <param name="sizePercent">Size of the entity relative to its canvas.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// text entity object.</param>
        /// <returns>The ID of the text entity object.</returns>
        public static TextEntity Create(BaseEntity parent, string text, int fontSize,
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

            WorldEngine.Entity.BaseEntity pCE = EntityAPIHelper.GetPrivateEntity(parent);
            if (pCE == null)
            {
                Logging.LogWarning("[TextEntity->Create] Invalid parent entity.");
                return null;
            }
            if (pCE is not WorldEngine.Entity.UIEntity)
            {
                Logging.LogWarning("[TextEntity->Create] Parent entity not UI element.");
                return null;
            }

            UnityEngine.Vector2 pos = new UnityEngine.Vector2(positionPercent.x, positionPercent.y);
            UnityEngine.Vector2 size = new UnityEngine.Vector2(sizePercent.x, sizePercent.y);

            TextEntity te = new TextEntity();

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                te.internalEntity = WorldEngine.WorldEngine.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(te.internalEntity, te);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { te });
                }
            };

            WorldEngine.WorldEngine.ActiveWorld.entityManager.LoadTextEntity(text, fontSize,
                (WorldEngine.Entity.UIEntity) pCE, pos, size, guid, tag, onLoadAction);

            return te;
        }

        internal TextEntity()
        {
            internalEntityType = typeof(WorldEngine.Entity.TextEntity);
        }
        
        /// <summary>
        /// Get the text for the text entity.
        /// </summary>
        /// <returns>The text for the text entity.</returns>
        public string GetText()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetText] Unknown entity.");
                return null;
            }

            return ((WorldEngine.Entity.TextEntity) internalEntity).GetText();
        }

        /// <summary>
        /// Get the font size for the text entity.
        /// </summary>
        /// <returns>The font size for the text entity.</returns>
        public int GetFontSize()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetFontSize] Unknown entity.");
                return 0;
            }

            return ((WorldEngine.Entity.TextEntity) internalEntity).GetFontSize();
        }

        /// <summary>
        /// Get the color for the text entity.
        /// </summary>
        /// <returns>The color for the text entity.</returns>
        public Color GetColor()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetColor] Unknown entity.");
                return null;
            }

            UnityEngine.Color32 color = ((WorldEngine.Entity.TextEntity) internalEntity).GetColor();
            return new Color(color.r, color.g, color.b, color.a);
        }

        /// <summary>
        /// Set the text for the text entity.
        /// </summary>
        /// <param name="text">Text to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetText(string text)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetText] Unknown entity.");
                return false;
            }

            return ((WorldEngine.Entity.TextEntity) internalEntity).SetText(text);
        }

        /// <summary>
        /// Set the font size for the text entity.
        /// </summary>
        /// <param name="size">Size to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetFontSize(int size)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetFontSize] Unknown entity.");
                return false;
            }

            return ((WorldEngine.Entity.TextEntity) internalEntity).SetFontSize(size);
        }

        /// <summary>
        /// Set the color for the text entity.
        /// </summary>
        /// <param name="color">Color to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetColor(Color color)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetColor] Unknown entity.");
                return false;
            }

            UnityEngine.Color32 col = new UnityEngine.Color(color.r, color.g, color.b, color.a);
            return ((WorldEngine.Entity.TextEntity) internalEntity).SetColor(col);
        }

        /// <summary>
        /// Set the margins for the text entity.
        /// </summary>
        /// <param name="margins">Margins to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetMargins(Vector4 margins)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetMargins] Unknown entity.");
                return false;
            }

            UnityEngine.Vector4 marg = new UnityEngine.Vector4(margins.x, margins.y, margins.z, margins.w);
            return ((WorldEngine.Entity.TextEntity) internalEntity).SetMargins(marg);
        }

        /// <summary>
        /// Get the margins for the text entity.
        /// </summary>
        /// <returns>The margins for the text entity.</returns>
        public Vector4 GetMargins()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetMargins] Unknown entity.");
                return null;
            }

            UnityEngine.Vector4 margins = ((WorldEngine.Entity.TextEntity) internalEntity).GetMargins();
            return new Vector4(margins.x, margins.y, margins.z, margins.w);
        }
    }
}