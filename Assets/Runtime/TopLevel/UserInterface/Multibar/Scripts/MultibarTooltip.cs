// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using TMPro;
using UnityEngine;

namespace FiveSQD.WebVerse.Interface.MultibarMenu
{
    /// <summary>
    /// Class for multibar tooltip.
    /// </summary>
    public class MultibarTooltip : MonoBehaviour
    {
        /// <summary>
        /// The rect transform.
        /// </summary>
        public RectTransform rectTransform;

        /// <summary>
        /// Tooltip text.
        /// </summary>
        public TMP_Text tooltipText;

        /// <summary>
        /// Enable the tooltip.
        /// </summary>
        /// <param name="tooltip">Tooltip text to show.</param>
        public void EnableTooltip(string tooltip)
        {
            tooltipText.text = tooltip;
            foreach (MultibarTooltip mtt in FindObjectsByType<MultibarTooltip>(FindObjectsSortMode.None))
            {
                mtt.gameObject.SetActive(false);
            }
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Disable the tooltip.
        /// </summary>
        public void DisableTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}