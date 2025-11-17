// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.EventSystems;


namespace FiveSQD.WebVerse.Interface.MultibarMenu
{
    /// <summary>
    /// Class for multibar tooltippable UI object,
    /// </summary>
    public class MultibarTooltippable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// Tooltip.
        /// </summary>
        public MultibarTooltip tooltip;

        /// <summary>
        /// Text to show.
        /// </summary>
        public string tooltipText;

        /// <summary>
        /// Threshold for activation in seconds.
        /// </summary>
        public float hoverActivationThreshold = 1;

        /// <summary>
        /// Current hover time in seconds.
        /// </summary>
        private float hoverTime = -1;

        /// <summary>
        /// Whether or not tooltip is activated.
        /// </summary>
        private bool tooltipActivated = false;

        /// <summary>
        /// Called when pointer enters.
        /// </summary>
        /// <param name="data">Pointer event data.</param>
        public void OnPointerEnter(PointerEventData data)
        {
            hoverTime = 0;
        }

        /// <summary>
        /// Called when pointer enters.
        /// </summary>
        /// <param name="data">Pointer event data.</param>
        public void OnPointerExit(PointerEventData data)
        {
            hoverTime -= 1;
            if (tooltipActivated)
            {
                tooltip.DisableTooltip();
            }
        }

        private void Update()
        {
            if (hoverTime > -1)
            {
                hoverTime += Time.deltaTime;
            }

            if (hoverTime > hoverActivationThreshold)
            {
                hoverTime = -1;
                tooltipActivated = true;
                tooltip.EnableTooltip(tooltipText);
            }
        }
    }
}