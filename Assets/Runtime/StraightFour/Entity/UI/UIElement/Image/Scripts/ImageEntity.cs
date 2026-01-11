// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for an image entity.
    /// </summary>
    public class ImageEntity : UIElementEntity
    {
        /// <summary>
        /// Canvas object for the image entity.
        /// </summary>
        public Canvas canvasObject;

        /// <summary>
        /// Image object for the image entity.
        /// </summary>
        private RawImage imageObject;

        /// <summary>
        /// Set the texture for the image entity.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public bool SetTexture(Texture texture)
        {
            imageObject.texture = texture;

            return true;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /// <param name="parentCanvas">Canvas to place the image entity on.</param>
        public override void Initialize(System.Guid idToSet, UIEntity parentCanvas)
        {
            base.Initialize(idToSet, parentCanvas);

            canvasObject = gameObject.AddComponent<Canvas>();
            imageObject = gameObject.AddComponent<RawImage>();

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

            MakeHidden();
        }
    }
}