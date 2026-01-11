// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using TMPro;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for a text entity.
    /// </summary>
    public class TextEntity : UIElementEntity
    {
        /// <summary>
        /// Canvas object for the text entity.
        /// </summary>
        public Canvas canvasObject;

        /// <summary>
        /// Text object for the text entity.
        /// </summary>
        public TextMeshProUGUI textObject;

        /// <summary>
        /// Scale factor for the text entity.
        /// </summary>
        private static readonly float scaleFactor = 1;

        /// <summary>
        /// Get the text for the text entity.
        /// </summary>
        /// <returns>The text for the text entity.</returns>
        public string GetText()
        {
            return textObject.text;
        }

        /// <summary>
        /// Get the font size for the text entity.
        /// </summary>
        /// <returns>The font size for the text entity.</returns>
        public int GetFontSize()
        {
            return (int) (textObject.fontSize / scaleFactor);
        }

        /// <summary>
        /// Get the color for the text entity.
        /// </summary>
        /// <returns>The color for the text entity.</returns>
        public Color GetColor()
        {
            return textObject.color;
        }

        /// <summary>
        /// Set the text for the text entity.
        /// </summary>
        /// <param name="text">Text to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetText(string text)
        {
            textObject.text = text;

            return true;
        }

        /// <summary>
        /// Set the font size for the text entity.
        /// </summary>
        /// <param name="size">Size to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetFontSize(int size)
        {
            textObject.fontSize = size * scaleFactor;

            return true;
        }

        /// <summary>
        /// Set the color for the text entity.
        /// </summary>
        /// <param name="color">Color to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetColor(Color color)
        {
            textObject.color = color;

            return true;
        }

        /// <summary>
        /// Set the margins for the text entity.
        /// </summary>
        /// <param name="margins">Margins to apply to the text entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetMargins(Vector4 margins)
        {
            textObject.margin = margins;

            return true;
        }

        /// <summary>
        /// Get the margins for the text entity.
        /// </summary>
        /// <returns>The margins for the text entity.</returns>
        public Vector4 GetMargins()
        {
            return textObject.margin;
        }

        /// <summary>
        /// Set the font for the text entity.
        /// </summary>
        /// <param name="fontName">Name of the font to apply.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetFont(string fontName)
        {
            if (string.IsNullOrEmpty(fontName))
            {
                Utilities.LogSystem.LogWarning("[TextEntity->SetFont] Font name cannot be null or empty.");
                return false;
            }

            // Try to load font from Resources
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/" + fontName);
            if (font == null)
            {
                // Try to load from default Unity fonts
                font = Resources.GetBuiltinResource<TMP_FontAsset>("LegacyRuntime.ttf");
                if (font == null)
                {
                    Utilities.LogSystem.LogError($"[TextEntity->SetFont] Font '{fontName}' not found.");
                    return false;
                }
            }

            textObject.font = font;
            return true;
        }

        /// <summary>
        /// Get the current font name for the text entity.
        /// </summary>
        /// <returns>The current font name.</returns>
        public string GetFont()
        {
            if (textObject.font == null)
            {
                return "Default";
            }
            return textObject.font.name;
        }

        /// <summary>
        /// Set the bold style for the text entity.
        /// </summary>
        /// <param name="bold">Whether to enable bold.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetBold(bool bold)
        {
            if (bold)
            {
                textObject.fontStyle |= FontStyles.Bold;
            }
            else
            {
                textObject.fontStyle &= ~FontStyles.Bold;
            }
            return true;
        }

        /// <summary>
        /// Get whether the text entity is bold.
        /// </summary>
        /// <returns>Whether the text is bold.</returns>
        public bool GetBold()
        {
            return (textObject.fontStyle & FontStyles.Bold) == FontStyles.Bold;
        }

        /// <summary>
        /// Set the italic style for the text entity.
        /// </summary>
        /// <param name="italic">Whether to enable italic.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetItalic(bool italic)
        {
            if (italic)
            {
                textObject.fontStyle |= FontStyles.Italic;
            }
            else
            {
                textObject.fontStyle &= ~FontStyles.Italic;
            }
            return true;
        }

        /// <summary>
        /// Get whether the text entity is italic.
        /// </summary>
        /// <returns>Whether the text is italic.</returns>
        public bool GetItalic()
        {
            return (textObject.fontStyle & FontStyles.Italic) == FontStyles.Italic;
        }

        /// <summary>
        /// Set the underline style for the text entity.
        /// </summary>
        /// <param name="underline">Whether to enable underline.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetUnderline(bool underline)
        {
            if (underline)
            {
                textObject.fontStyle |= FontStyles.Underline;
            }
            else
            {
                textObject.fontStyle &= ~FontStyles.Underline;
            }
            return true;
        }

        /// <summary>
        /// Get whether the text entity is underlined.
        /// </summary>
        /// <returns>Whether the text is underlined.</returns>
        public bool GetUnderline()
        {
            return (textObject.fontStyle & FontStyles.Underline) == FontStyles.Underline;
        }

        /// <summary>
        /// Set the strikethrough style for the text entity.
        /// </summary>
        /// <param name="strikethrough">Whether to enable strikethrough.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetStrikethrough(bool strikethrough)
        {
            if (strikethrough)
            {
                textObject.fontStyle |= FontStyles.Strikethrough;
            }
            else
            {
                textObject.fontStyle &= ~FontStyles.Strikethrough;
            }
            return true;
        }

        /// <summary>
        /// Get whether the text entity has strikethrough.
        /// </summary>
        /// <returns>Whether the text has strikethrough.</returns>
        public bool GetStrikethrough()
        {
            return (textObject.fontStyle & FontStyles.Strikethrough) == FontStyles.Strikethrough;
        }

        /// <summary>
        /// Set the text alignment for the text entity.
        /// </summary>
        /// <param name="alignment">Text alignment to set.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetTextAlignment(TextAlignment alignment)
        {
            TextAlignmentOptions textAlign;

            switch (alignment)
            {
                case TextAlignment.Center:
                    textAlign = TextAlignmentOptions.Center;
                    break;
                case TextAlignment.Left:
                    textAlign = TextAlignmentOptions.Left;
                    break;
                case TextAlignment.Right:
                    textAlign = TextAlignmentOptions.Right;
                    break;
                case TextAlignment.Middle:
                    textAlign = TextAlignmentOptions.Midline;
                    break;
                case TextAlignment.Top:
                    textAlign = TextAlignmentOptions.Top;
                    break;
                case TextAlignment.Bottom:
                    textAlign = TextAlignmentOptions.Bottom;
                    break;
                default:
                    Utilities.LogSystem.LogWarning("[TextEntity->SetTextAlignment] Unknown text alignment.");
                    return false;
            }

            textObject.alignment = textAlign;
            return true;
        }

        /// <summary>
        /// Get the text alignment for the text entity.
        /// </summary>
        /// <returns>The current text alignment.</returns>
        public TextAlignment GetTextAlignment()
        {
            switch (textObject.alignment)
            {
                case TextAlignmentOptions.Center:
                    return TextAlignment.Center;
                case TextAlignmentOptions.Midline:
                    return TextAlignment.Middle;
                case TextAlignmentOptions.Left:
                case TextAlignmentOptions.TopLeft:
                case TextAlignmentOptions.MidlineLeft:
                case TextAlignmentOptions.BottomLeft:
                    return TextAlignment.Left;
                case TextAlignmentOptions.Right:
                case TextAlignmentOptions.TopRight:
                case TextAlignmentOptions.MidlineRight:
                case TextAlignmentOptions.BottomRight:
                    return TextAlignment.Right;
                case TextAlignmentOptions.Top:
                case TextAlignmentOptions.TopJustified:
                    return TextAlignment.Top;
                case TextAlignmentOptions.Bottom:
                case TextAlignmentOptions.BottomJustified:
                    return TextAlignment.Bottom;
                default:
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
            switch (wrapping)
            {
                case TextWrapping.NoWrap:
                    textObject.textWrappingMode = TextWrappingModes.NoWrap;
                    break;
                case TextWrapping.Wrap:
                    textObject.textWrappingMode = TextWrappingModes.Normal;
                    break;
                default:
                    Utilities.LogSystem.LogWarning("[TextEntity->SetTextWrapping] Unknown text wrapping option.");
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get the text wrapping for the text entity.
        /// </summary>
        /// <returns>The current text wrapping setting.</returns>
        public TextWrapping GetTextWrapping()
        {
            return textObject.textWrappingMode == TextWrappingModes.Normal ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /// <param name="parentCanvas">Canvas to place the text entity on.</param>
        public override void Initialize(System.Guid idToSet, UIEntity parentCanvas)
        {
            base.Initialize(idToSet, parentCanvas);

            canvasObject = gameObject.AddComponent<Canvas>();
            textObject = gameObject.AddComponent<TextMeshProUGUI>();

            RectTransform rt = gameObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = gameObject.AddComponent<RectTransform>();
            }
            rt.position = Vector3.zero;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            uiElementRectTransform = rt;

            if (parentCanvas != null)
            {
                SetParent(parentCanvas);
            }

            SetColor(Color.black);

            MakeHidden();
        }
    }
}