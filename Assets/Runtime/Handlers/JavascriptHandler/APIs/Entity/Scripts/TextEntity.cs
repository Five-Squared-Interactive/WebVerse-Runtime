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

            StraightFour.Entity.BaseEntity pCE = EntityAPIHelper.GetPrivateEntity(parent);
            if (pCE == null)
            {
                Logging.LogWarning("[TextEntity->Create] Invalid parent entity.");
                return null;
            }
            if (pCE is not StraightFour.Entity.UIEntity)
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
                te.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(te.internalEntity, te);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { te });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadTextEntity(text, fontSize,
                (StraightFour.Entity.UIEntity) pCE, pos, size, guid, tag, onLoadAction);

            return te;
        }

        /// <summary>
        /// Create a text entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the text entity configuration.</param>
        /// <param name="parent">Parent entity for the text entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created text entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, textEntity) =>
            {
                if (!success || textEntity == null || !(textEntity is StraightFour.Entity.TextEntity))
                {
                    Logging.LogError("[TextEntity:Create] Error loading text entity from JSON.");
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
                                (StraightFour.Entity.TextEntity) textEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadTextEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal TextEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.TextEntity);
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

            return ((StraightFour.Entity.TextEntity) internalEntity).GetText();
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

            return ((StraightFour.Entity.TextEntity) internalEntity).GetFontSize();
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

            UnityEngine.Color32 color = ((StraightFour.Entity.TextEntity) internalEntity).GetColor();
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

            return ((StraightFour.Entity.TextEntity) internalEntity).SetText(text);
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

            return ((StraightFour.Entity.TextEntity) internalEntity).SetFontSize(size);
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
            return ((StraightFour.Entity.TextEntity) internalEntity).SetColor(col);
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
            return ((StraightFour.Entity.TextEntity) internalEntity).SetMargins(marg);
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

            UnityEngine.Vector4 margins = ((StraightFour.Entity.TextEntity) internalEntity).GetMargins();
            return new Vector4(margins.x, margins.y, margins.z, margins.w);
        }

        /// <summary>
        /// Set the font for the text entity.
        /// </summary>
        /// <param name="fontName">Name of the font to apply.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetFont(string fontName)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetFont] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).SetFont(fontName);
        }

        /// <summary>
        /// Get the current font name for the text entity.
        /// </summary>
        /// <returns>The current font name.</returns>
        public string GetFont()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetFont] Unknown entity.");
                return null;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).GetFont();
        }

        /// <summary>
        /// Set the bold style for the text entity.
        /// </summary>
        /// <param name="bold">Whether to enable bold.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetBold(bool bold)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetBold] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).SetBold(bold);
        }

        /// <summary>
        /// Get whether the text entity is bold.
        /// </summary>
        /// <returns>Whether the text is bold.</returns>
        public bool GetBold()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetBold] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).GetBold();
        }

        /// <summary>
        /// Set the italic style for the text entity.
        /// </summary>
        /// <param name="italic">Whether to enable italic.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetItalic(bool italic)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetItalic] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).SetItalic(italic);
        }

        /// <summary>
        /// Get whether the text entity is italic.
        /// </summary>
        /// <returns>Whether the text is italic.</returns>
        public bool GetItalic()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetItalic] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).GetItalic();
        }

        /// <summary>
        /// Set the underline style for the text entity.
        /// </summary>
        /// <param name="underline">Whether to enable underline.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetUnderline(bool underline)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetUnderline] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).SetUnderline(underline);
        }

        /// <summary>
        /// Get whether the text entity is underlined.
        /// </summary>
        /// <returns>Whether the text is underlined.</returns>
        public bool GetUnderline()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetUnderline] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).GetUnderline();
        }

        /// <summary>
        /// Set the strikethrough style for the text entity.
        /// </summary>
        /// <param name="strikethrough">Whether to enable strikethrough.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetStrikethrough(bool strikethrough)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetStrikethrough] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).SetStrikethrough(strikethrough);
        }

        /// <summary>
        /// Get whether the text entity has strikethrough.
        /// </summary>
        /// <returns>Whether the text has strikethrough.</returns>
        public bool GetStrikethrough()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetStrikethrough] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.TextEntity) internalEntity).GetStrikethrough();
        }

        /// <summary>
        /// Set the text alignment for the text entity.
        /// </summary>
        /// <param name="alignment">Text alignment to set.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetTextAlignment(TextAlignment alignment)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetTextAlignment] Unknown entity.");
                return false;
            }

            StraightFour.Entity.TextAlignment convertedAlignment = StraightFour.Entity.TextAlignment.Left;
            switch (alignment)
            {
                case TextAlignment.Center:
                    convertedAlignment = StraightFour.Entity.TextAlignment.Center;
                    break;

                case TextAlignment.Left:
                    convertedAlignment = StraightFour.Entity.TextAlignment.Left;
                    break;

                case TextAlignment.Right:
                    convertedAlignment = StraightFour.Entity.TextAlignment.Right;
                    break;

                case TextAlignment.Top:
                    convertedAlignment = StraightFour.Entity.TextAlignment.Top;
                    break;

                case TextAlignment.Bottom:
                    convertedAlignment = StraightFour.Entity.TextAlignment.Bottom;
                    break;

                default:
                    Logging.LogWarning("[TextEntity:SetTextAlignment] Unknown text alignment.");
                    return false;
            }

            return ((StraightFour.Entity.TextEntity)internalEntity).SetTextAlignment(convertedAlignment);
        }

        /// <summary>
        /// Get the text alignment for the text entity.
        /// </summary>
        /// <returns>The current text alignment.</returns>
        public TextAlignment GetTextAlignment()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetTextAlignment] Unknown entity.");
                return TextAlignment.Left;
            }

            StraightFour.Entity.TextAlignment rawAlignment = ((StraightFour.Entity.TextEntity)internalEntity).GetTextAlignment();

            switch (rawAlignment)
            {
                case StraightFour.Entity.TextAlignment.Center:
                    return TextAlignment.Center;

                case StraightFour.Entity.TextAlignment.Left:
                    return TextAlignment.Left;

                case StraightFour.Entity.TextAlignment.Right:
                    return TextAlignment.Right;

                case StraightFour.Entity.TextAlignment.Top:
                    return TextAlignment.Top;

                case StraightFour.Entity.TextAlignment.Bottom:
                    return TextAlignment.Bottom;

                default:
                    Logging.LogWarning("[TextEntity:GetTextAlignment] Unknown text alignment.");
                    return TextAlignment.Left;
            }
        }

        /// <summary>
        /// Set the text wrapping for the text entity.
        /// </summary>
        /// <param name="wrapping">Text wrapping to set.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetTextWrapping(TextWrapping wrapping)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetTextWrapping] Unknown entity.");
                return false;
            }
            
            StraightFour.Entity.TextWrapping convertedWrapping = StraightFour.Entity.TextWrapping.NoWrap;
            switch (wrapping)
            {
                case TextWrapping.NoWrap:
                    convertedWrapping = StraightFour.Entity.TextWrapping.NoWrap;
                    break;

                case TextWrapping.Wrap:
                    convertedWrapping = StraightFour.Entity.TextWrapping.Wrap;
                    break;

                default:
                    Logging.LogWarning("[TextEntity:SetTextWrapping] Unknown text wrapping.");
                    return false;
            }

            return ((StraightFour.Entity.TextEntity)internalEntity).SetTextWrapping(convertedWrapping);
        }

        /// <summary>
        /// Get the text wrapping for the text entity.
        /// </summary>
        /// <returns>The current text wrapping setting.</returns>
        public TextWrapping GetTextWrapping()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:GetTextWrapping] Unknown entity.");
                return TextWrapping.NoWrap;
            }

            StraightFour.Entity.TextWrapping rawWrapping = ((StraightFour.Entity.TextEntity)internalEntity).GetTextWrapping();

            switch (rawWrapping)
            {
                case StraightFour.Entity.TextWrapping.NoWrap:
                    return TextWrapping.NoWrap;

                case StraightFour.Entity.TextWrapping.Wrap:
                    return TextWrapping.Wrap;

                default:
                    Logging.LogWarning("[TextEntity:GetTextWrapping] Unknown text wrapping.");
                    return TextWrapping.NoWrap;
            }
        }

        /// <summary>
        /// Stretch the text entity to fill its parent.
        /// </summary>
        /// <param name="stretch">Whether to stretch to parent. If false, restores normal sizing.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool StretchToParent(bool stretch = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:StretchToParent] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.UIElementEntity) internalEntity).StretchToParent(stretch);
        }

        /// <summary>
        /// Set the alignment of the text entity within its parent.
        /// </summary>
        /// <param name="alignment">Alignment to set.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetAlignment(UIElementAlignment alignment)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TextEntity:SetAlignment] Unknown entity.");
                return false;
            }
            
            StraightFour.Entity.UIElementAlignment convertedAlignment = StraightFour.Entity.UIElementAlignment.Center;
            switch (alignment)
            {
                case UIElementAlignment.Center:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Center;
                    break;

                case UIElementAlignment.Left:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Left;
                    break;

                case UIElementAlignment.Right:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Right;
                    break;

                case UIElementAlignment.Top:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Top;
                    break;

                case UIElementAlignment.Bottom:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Bottom;
                    break;

                default:
                    Logging.LogError("[ButtonEntity:SetAlignment] Invalid alignment.");
                    return false;
            }

            return ((StraightFour.Entity.UIElementEntity)internalEntity).SetAlignment(convertedAlignment);
        }
    }
}