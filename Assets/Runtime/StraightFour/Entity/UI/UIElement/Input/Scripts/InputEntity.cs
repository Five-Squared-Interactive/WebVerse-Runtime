// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for an input entity.
    /// </summary>
    public class InputEntity : UIElementEntity
    {
        /// <summary>
        /// Canvas object for the input entity.
        /// </summary>
        public Canvas canvasObject;

        /// <summary>
        /// Image object for the input entity.
        /// </summary>
        public Image imageObject;

        /// <summary>
        /// Input object for the input entity.
        /// </summary>
        public TMP_InputField inputObject;

        /// <summary>
        /// Get the text for the input entity.
        /// </summary>
        /// <returns>Text of the input entity.</returns>
        public string GetText()
        {
            return inputObject.text;
        }

        /// <summary>
        /// Set the text for the input entity.
        /// </summary>
        /// <param name="text">Text for the input entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetText(string text)
        {
            inputObject.text = text;

            return true;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /// <param name="parentCanvas">Canvas to place the input entity on.</param>
        public override void Initialize(System.Guid idToSet, UIEntity parentCanvas)
        {
            base.Initialize(idToSet, parentCanvas);

            //GameObject inputGO = Instantiate(StraightFour.ActiveWorld.entityManager.inputEntityPrefab);
            //inputGO.transform.SetParent(transform);

            canvasObject = gameObject.GetComponent<Canvas>();
            imageObject = gameObject.GetComponentInChildren<Image>();
            inputObject = gameObject.GetComponentInChildren<TMP_InputField>();

            RectTransform rt = gameObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = gameObject.AddComponent<RectTransform>();
            }
            rt.position = Vector3.zero;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            uiElementRectTransform = rt;

            GraphicRaycaster graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            if (parentCanvas != null)
            {
                SetParent(parentCanvas);
            }

            MakeHidden();
        }
    }
}