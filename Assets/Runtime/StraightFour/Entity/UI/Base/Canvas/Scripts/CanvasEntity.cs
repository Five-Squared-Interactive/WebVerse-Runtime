// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.StraightFour.Utilities;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for a canvas entity.
    /// </summary>
    public class CanvasEntity : UIEntity
    {
        /// <summary>
        /// Canvas object reference.
        /// </summary>
        public Canvas canvasObject;

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(System.Guid idToSet)
        {
            base.Initialize(idToSet);

            canvasObject = gameObject.AddComponent<Canvas>();

            RectTransform rt = gameObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = gameObject.gameObject.AddComponent<RectTransform>();
            }
            rt.position = Vector3.zero;
            rt.anchorMin = rt.anchorMax = Vector2.zero;

            gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            gameObject.AddComponent<GraphicRaycaster>();

            MakeHidden();
        }

        /// <summary>
        /// Set the position of the entity.
        /// </summary>
        /// <param name="position">Position to set.</param>
        /// <param name="local">Whether or not the position is local.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetPosition(Vector3 position, bool local, bool synchronize = true)
        {
            if (IsScreenCanvas())
            {
                LogSystem.LogWarning("[CanvasEntity->SetPosition] Cannot set position of a screen canvas entity.");
                return false;
            }

            if (local)
            {
                transform.localPosition = position;
            }
            else
            {
                Vector3 worldOffset = StraightFour.ActiveWorld.worldOffset;
                transform.position = new Vector3(position.x + worldOffset.x,
                    position.y + worldOffset.y, position.z + worldOffset.z);;
            }

            if (synchronize && synchronizer != null && positionUpdateTime > minUpdateTime)
            {
                synchronizer.SetPosition(this, position);
                positionUpdateTime = 0;
            }

            return true;
        }

        /// <summary>
        /// Set the size of the entity. This method cannot be used to set the size of a canvas entity; a Vector2 must be used.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetSize(Vector3 size, bool synchronize = true)
        {
            LogSystem.LogWarning("[CanvasEntity->SetSize] Cannot set size of a canvas entity using a Vector3.");

            return false;
        }

        /// <summary>
        /// Set the size of the entity.
        /// Must be implemented by inheriting classes, as the size of an entity is dependent
        /// on its type.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetSize(Vector2 size, bool synchronize = true)
        {
            if (IsScreenCanvas())
            {
                LogSystem.LogWarning("[CanvasEntity->SetSize] Cannot set size of a screen canvas entity.");
                return false;
            }

            RectTransform rt = gameObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                LogSystem.LogError("[CanvasEntity->SetSize] No rect transform.");
                return false;
            }

            rt.sizeDelta = size;

            if (synchronize && synchronizer != null && sizeUpdateTime > minUpdateTime)
            {
                synchronizer.SetSize(this, size);
                sizeUpdateTime = 0;
            }

            return true;
        }

        /// <summary>
        /// Get the position of the entity.
        /// </summary>
        /// <param name="local">Whether or not to provide the local position.</param>
        /// <returns>The position of the entity.</returns>
        public override Vector3 GetPosition(bool local)
        {
            if (IsScreenCanvas())
            {
                LogSystem.LogWarning("[CanvasEntity->GetPosition] Cannot get position of a screen canvas entity.");
                return Vector3.zero;
            }

            return local ? transform.localPosition : transform.position;
        }

        /// <summary>
        /// Get the size of the entity.
        /// Must be implemented by inheriting classes, as the size of an entity is dependent
        /// on its type.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public override Vector3 GetSize()
        {
            if (IsScreenCanvas())
            {
                LogSystem.LogWarning("[CanvasEntity->GetPosition] Cannot get size of a screen canvas entity.");
                return Vector3.zero;
            }

            RectTransform rt = gameObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                LogSystem.LogError("[CanvasEntity->GetSize] No rect transform.");
                return Vector3.zero;
            }

            return rt.sizeDelta;
        }

        /// <summary>
        /// Make the canvas a world canvas.
        /// </summary>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool MakeWorldCanvas(bool synchronize = true)
        {
            if (canvasObject == null)
            {
                LogSystem.LogError("[CanvasEntity->MakeWorldCanvas] No canvas object.");
                return false;
            }

            canvasObject.renderMode = RenderMode.WorldSpace;

            if (synchronize && synchronizer != null)
            {
                synchronizer.MakeWorldCanvas(this);
            }

            return true;
        }

        /// <summary>
        /// Make the canvas a screen canvas.
        /// </summary>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool MakeScreenCanvas(bool synchronize = true)
        {
            if (canvasObject == null)
            {
                LogSystem.LogError("[CanvasEntity->MakeScreenCanvas] No canvas object.");
                return false;
            }

            canvasObject.renderMode = RenderMode.ScreenSpaceOverlay;

            if (synchronize && synchronizer != null)
            {
                synchronizer.MakeScreenCanvas(this);
            }

            return true;
        }

        /// <summary>
        /// Returns whether or not the canvas entity is a screen canvas.
        /// </summary>
        /// <returns>Whether or not the canvas entity is a screen canvas.</returns>
        public bool IsScreenCanvas()
        {
            if (canvasObject == null)
            {
                LogSystem.LogError("[CanvasEntity->IsScreenCanvas] No canvas object.");
                return false;
            }

            return canvasObject.renderMode == RenderMode.ScreenSpaceOverlay;
        }
    }
}