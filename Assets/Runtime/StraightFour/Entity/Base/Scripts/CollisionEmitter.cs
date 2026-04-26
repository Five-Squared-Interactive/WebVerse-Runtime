// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// MonoBehaviour component that detects Unity collision and trigger events
    /// and forwards them via C# Action callbacks. The owning layer (e.g. the
    /// Javascript handler) subscribes to OnCollisionEnterEvent / OnCollisionExitEvent
    /// to bridge into its own event system.
    /// Attach to entity GameObjects that have Rigidbody and Collider components.
    /// </summary>
    public class CollisionEmitter : MonoBehaviour
    {
        /// <summary>
        /// Reference to the owning StraightFour entity.
        /// Set when the component is attached during entity initialization.
        /// </summary>
        internal BaseEntity ownerEntity;

        /// <summary>
        /// Fired on collision/trigger enter. Parameters: ownerEntity, otherGameObject.
        /// </summary>
        public Action<BaseEntity, GameObject> OnCollisionEnterEvent;

        /// <summary>
        /// Fired on collision/trigger exit. Parameters: ownerEntity, otherGameObject.
        /// </summary>
        public Action<BaseEntity, GameObject> OnCollisionExitEvent;

        private void OnDestroy()
        {
            ownerEntity = null;
            OnCollisionEnterEvent = null;
            OnCollisionExitEvent = null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (ownerEntity != null)
                OnCollisionEnterEvent?.Invoke(ownerEntity, collision.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (ownerEntity != null)
                OnCollisionExitEvent?.Invoke(ownerEntity, collision.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ownerEntity != null)
                OnCollisionEnterEvent?.Invoke(ownerEntity, other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (ownerEntity != null)
                OnCollisionExitEvent?.Invoke(ownerEntity, other.gameObject);
        }
    }
}
