// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using FiveSQD.StraightFour.Utilities;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Base class for a UI element entity.
    /// </summary>
    public class UIElementEntity : UIEntity
    {
        protected RectTransform uiElementRectTransform;

        protected Vector2 targetPosition;

        protected Vector2 targetSize;

        /// <summary>
        /// Set the parent of the entity. This method is not valid for UI element entities.
        /// </summary>
        /// <param name="parent">Parent to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetParent(BaseEntity parent)
        {
            LogSystem.LogError("[UIElementEntity->SetParent] Parent on a UI element entity must be set using a UI entity.");
            return false;
        }

        /// <summary>
        /// Set the parent of the entity.
        /// </summary>
        /// <param name="parent">Parent to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetParent(UIEntity parent)
        {
            if (parent == null)
            {
                LogSystem.LogError("[UIElementEntity->SetParent] UI Element cannot have a null parent.");
                return false;
            }

            transform.SetParent(parent.transform);
            if (synchronizer != null)
            {
                synchronizer.SetParent(this, parent);
            }

            return true;
        }

        /// <summary>
        /// Get the size of the UI element entity as a percentage of its canvas.
        /// </summary>
        /// <returns>The size of the UI element entity as a percentage of its canvas.</returns>
        public Vector2 GetSizePercent()
        {
            RectTransform rt = uiElementRectTransform;
            if (rt == null)
            {
                LogSystem.LogWarning("[UIElementEntity->GetSizePercent] No rect transform.");
                return Vector3.zero;
            }

            CanvasEntity parentCanvasEntity = GetParentCanvasEntity();
            if (parentCanvasEntity == null)
            {
                LogSystem.LogError("[UIElementEntity->GetSizePercent] No parent canvas entity.");
                return Vector3.zero;
            }

            RectTransform parentRT = parentCanvasEntity.GetComponent<RectTransform>();
            if (parentRT == null)
            {
                LogSystem.LogError("[UIElementEntity->GetSizePercent] No parent canvas entity rect transform.");
                return Vector3.zero;
            }

            return new Vector2(rt.sizeDelta.x / parentRT.sizeDelta.x, rt.sizeDelta.y / parentRT.sizeDelta.y);
        }

        /// <summary>
        /// Get the position of the entity. A UI element entity cannot have an absolute position.
        /// </summary>
        /// <param name="local">Whether or not to provide the local position.</param>
        /// <returns>The position of the entity.</returns>
        public override Vector3 GetPosition(bool local)
        {
            LogSystem.LogWarning("[UIElementEntity->GetPosition] Cannot get position of a UI element entity.");

            return Vector3.zero;
        }

        /// <summary>
        /// Get the position of the entity as a percentage of its canvas.
        /// </summary>
        /// <param name="local">Whether or not to provide the local position.</param>
        /// <returns>The position of the entity as a percentage of its canvas.</returns>
        public Vector2 GetPositionPercent()
        {
            RectTransform rt = uiElementRectTransform;
            if (rt == null)
            {
                LogSystem.LogWarning("[UIElementEntity->GetPositionPercent] No rect transform.");
                return Vector3.zero;
            }

            CanvasEntity parentCanvasEntity = GetParentCanvasEntity();
            if (parentCanvasEntity == null)
            {
                LogSystem.LogError("[UIElementEntity->GetPositionPercent] No parent canvas entity.");
                return Vector3.zero;
            }

            RectTransform parentRT = parentCanvasEntity.GetComponent<RectTransform>();
            if (parentRT == null)
            {
                LogSystem.LogError("[UIElementEntity->GetPositionPercent] No parent canvas entity rect transform.");
                return Vector3.zero;
            }

            return new Vector2((rt.anchoredPosition.x - rt.sizeDelta.x / 2) / parentRT.sizeDelta.x,
                ((-1 * rt.anchoredPosition.y) - rt.sizeDelta.y / 2) / parentRT.sizeDelta.y);
        }

        /// <summary>
        /// Set the size of the entity as a percentage of its canvas.
        /// </summary>
        /// <param name="percent">Percent to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetSizePercent(Vector2 percent, bool synchronize = true)
        {
            targetSize = percent;

            return CorrectSizeAndPosition(Screen.width, Screen.height);
        }

        /// <summary>
        /// Set the position of the entity. A UI element entity cannot have an absolute position.
        /// </summary>
        /// <param name="position">Position to set.</param>
        /// <param name="local">Whether or not the position is local.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetPosition(Vector3 position, bool local, bool synchronize = true)
        {
            LogSystem.LogWarning("[UIElementEntity->SetPosition] Cannot set position of a UI element entity.");

            return false;
        }

        /// <summary>
        /// Set the position of the entity as a percentage of its canvas.
        /// </summary>
        /// <param name="percent">Percentage to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool SetPositionPercent(Vector2 percent, bool synchronize = true)
        {
            targetPosition = percent;

            return CorrectSizeAndPosition(Screen.width, Screen.height);
        }

        /// <summary>
        /// Set the rotation of the entity. Not a valid operation for UI element entities.
        /// </summary>
        /// <param name="rotation">Rotation to set.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetRotation(Quaternion rotation, bool local, bool synchronize = true)
        {
            LogSystem.LogError("[UIElementEntity->SetRotation] Cannot set rotation of a UI element entity.");

            return false;
        }

        /// <summary>
        /// Set the Euler rotation of the entity. Not valid for UI element entities.
        /// </summary>
        /// <param name="rotation">Rotation to set.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetEulerRotation(Vector3 rotation, bool local, bool synchronize = true)
        {
            LogSystem.LogError("[UIElementEntity->SetEulerRotation] Cannot set Euler rotation of a UI element entity.");

            return false;
        }

        /// <summary>
        /// Set the scale of the entity. Not a valid operation for UI element entities.
        /// </summary>
        /// <param name="scale">Scale to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetScale(Vector3 scale, bool synchronize = true)
        {
            LogSystem.LogError("[UIElementEntity->SetScale] Cannot set scale of a UI element entity.");

            return false;
        }

        /// <summary>
        /// Correct the size and position of the UI element entity.
        /// </summary>
        /// <param name="screenWidth">Width of the screen.</param>
        /// <param name="screenHeight">Height of the screen.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public virtual bool CorrectSizeAndPosition(float screenWidth, float screenHeight)
        {
            RectTransform rt = uiElementRectTransform;
            if (rt == null)
            {
                LogSystem.LogWarning("[UIElementEntity->CorrectSizeAndPosition] No rect transform.");
                return false;
            }

            CanvasEntity parentCanvasEntity = GetParentCanvasEntity();
            if (parentCanvasEntity == null)
            {
                LogSystem.LogError("[UIElementEntity->CorrectSizeAndPosition] No parent canvas entity.");
                return false;
            }

            UIEntity parentUIEntity = GetParentUIEntity();
            if (parentUIEntity == null)
            {
                LogSystem.LogError("[UIElementEntity->CorrectSizeAndPosition] No parent UI entity.");
                return false;
            }

            RectTransform parentRT = parentUIEntity.GetComponent<RectTransform>();
            if (parentRT == null)
            {
                LogSystem.LogError("[UIElementEntity->CorrectSizeAndPosition] No parent UI entity rect transform.");
                return false;
            }

            Vector2 worldSize = new Vector2(parentRT.sizeDelta.x * targetSize.x, parentRT.sizeDelta.y * targetSize.y);
            Vector3 worldPos = new Vector3(parentRT.sizeDelta.x * targetPosition.x + rt.sizeDelta.x / 2,
                -1 * parentRT.sizeDelta.y * targetPosition.y - rt.sizeDelta.y / 2);

            rt.sizeDelta = worldSize;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localPosition = Vector3.zero;
            rt.anchoredPosition = worldPos;

            return true;
        }

        /// <summary>
        /// Initialize this entity. This method cannot be used for a UI element entity; it must be called with a parent canvas.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(Guid idToSet)
        {
            LogSystem.LogError("[UIElementEntity->Initialize] UI element entity must be initialized with a parent canvas.");

            return;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public virtual void Initialize(Guid idToSet, UIEntity parentCanvas)
        {
            base.Initialize(idToSet);
        }

        /// <summary>
        /// Get the parent canvas entity of this entity.
        /// </summary>
        /// <returns>The parent canvas entity.</returns>
        protected CanvasEntity GetParentCanvasEntity()
        {
            return GetComponentInParent<CanvasEntity>(true);
        }

        /// <summary>
        /// Get the parent UI entity of this entity.
        /// </summary>
        /// <returns>The parent UI entity.</returns>
        protected UIEntity GetParentUIEntity()
        {
            if (transform.parent == null)
            {
                return null;
            }
            return transform.parent.GetComponentInParent<UIEntity>(true);
        }
        
        /// <summary>
		/// Set whether the UI element should stretch to fill its parent.
		/// </summary>
		/// <param name="stretch">Whether to stretch to parent. If false, restores normal sizing.</param>
		/// <param name="synchronize">Whether or not to synchronize the setting.</param>
		/// <returns>Whether or not the operation was successful.</returns>
		public virtual bool StretchToParent(bool stretch = true, bool synchronize = true)
		{
			RectTransform rt = uiElementRectTransform;
			if (rt == null)
			{
				LogSystem.LogWarning("[UIElementEntity->StretchToParent] No rect transform.");
				return false;
			}

			if (stretch)
			{
				// Set anchors to stretch to parent
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = Vector2.one;
				rt.anchoredPosition = Vector2.zero;
				rt.sizeDelta = Vector2.zero;

				if (synchronize && synchronizer != null)
				{
					// Synchronize with the network if needed
					synchronizer.SetPositionPercent(this, Vector2.zero);
					synchronizer.SetSizePercent(this, Vector2.one);
				}
			}
			else
			{
				// Restore to normal (centered) positioning with default size
				rt.anchorMin = new Vector2(0.5f, 0.5f);
				rt.anchorMax = new Vector2(0.5f, 0.5f);
				rt.pivot = new Vector2(0.5f, 0.5f);
				rt.anchoredPosition = Vector2.zero;
				
				// Set a reasonable default size (100x30 pixels)
				rt.sizeDelta = new Vector2(100f, 30f);

				if (synchronize && synchronizer != null)
				{
					// Synchronize with the network if needed
					synchronizer.SetPositionPercent(this, new Vector2(0.5f, 0.5f));
					synchronizer.SetSizePercent(this, new Vector2(0.1f, 0.1f)); // 10% of parent size as default
				}
			}

			return true;
		}

		/// <summary>
		/// Set the alignment of the UI element within its parent.
		/// </summary>
		/// <param name="alignment">Alignment to set.</param>
		/// <param name="synchronize">Whether or not to synchronize the setting.</param>
		/// <returns>Whether or not the operation was successful.</returns>
		public virtual bool SetAlignment(UIElementAlignment alignment, bool synchronize = true)
		{
			RectTransform rt = uiElementRectTransform;
			if (rt == null)
			{
				LogSystem.LogWarning("[UIElementEntity->SetAlignment] No rect transform.");
				return false;
			}

			Vector2 anchorMin, anchorMax, pivot;

			switch (alignment)
			{
				case UIElementAlignment.Center:
					anchorMin = anchorMax = new Vector2(0.5f, 0.5f);
					pivot = new Vector2(0.5f, 0.5f);
					break;
				case UIElementAlignment.Left:
					anchorMin = anchorMax = new Vector2(0f, 0.5f);
					pivot = new Vector2(0f, 0.5f);
					break;
				case UIElementAlignment.Right:
					anchorMin = anchorMax = new Vector2(1f, 0.5f);
					pivot = new Vector2(1f, 0.5f);
					break;
				case UIElementAlignment.Top:
					anchorMin = anchorMax = new Vector2(0.5f, 1f);
					pivot = new Vector2(0.5f, 1f);
					break;
				case UIElementAlignment.Bottom:
					anchorMin = anchorMax = new Vector2(0.5f, 0f);
					pivot = new Vector2(0.5f, 0f);
					break;
				default:
					LogSystem.LogWarning("[UIElementEntity->SetAlignment] Unknown alignment.");
					return false;
			}

			rt.anchorMin = anchorMin;
			rt.anchorMax = anchorMax;
			rt.pivot = pivot;
			rt.anchoredPosition = Vector2.zero;

			if (synchronize && synchronizer != null)
			{
				// Calculate position percentage based on alignment for synchronization
				Vector2 positionPercent = Vector2.zero;
				switch (alignment)
				{
					case UIElementAlignment.Center:
						positionPercent = new Vector2(0.5f, 0.5f);
						break;
					case UIElementAlignment.Left:
						positionPercent = new Vector2(0f, 0.5f);
						break;
					case UIElementAlignment.Right:
						positionPercent = new Vector2(1f, 0.5f);
						break;
					case UIElementAlignment.Top:
						positionPercent = new Vector2(0.5f, 1f);
						break;
					case UIElementAlignment.Bottom:
						positionPercent = new Vector2(0.5f, 0f);
						break;
				}
				synchronizer.SetPositionPercent(this, positionPercent);
			}

			return true;
		}
    }
}