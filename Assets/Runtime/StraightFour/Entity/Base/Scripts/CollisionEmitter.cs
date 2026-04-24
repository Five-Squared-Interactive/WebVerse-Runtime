// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// MonoBehaviour component that detects Unity collision and trigger events
    /// and emits them as World API events on the owning entity's IEventEmitter.
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
        /// Cleanup on destruction — prevent stale ownerEntity reference.
        /// </summary>
        private void OnDestroy()
        {
            ownerEntity = null;
        }

        /// <summary>
        /// Handle physics collision enter.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            EmitCollisionEvent(Events.Collision.Enter, collision.gameObject);
        }

        /// <summary>
        /// Handle physics collision exit.
        /// </summary>
        private void OnCollisionExit(Collision collision)
        {
            EmitCollisionEvent(Events.Collision.Exit, collision.gameObject);
        }

        /// <summary>
        /// Handle trigger enter (for trigger colliders).
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            EmitCollisionEvent(Events.Collision.Enter, other.gameObject);
        }

        /// <summary>
        /// Handle trigger exit (for trigger colliders).
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            EmitCollisionEvent(Events.Collision.Exit, other.gameObject);
        }

        /// <summary>
        /// Emit a collision event on the owning entity's JS API wrapper.
        /// Only emits if the entity has listeners for the collision event type.
        /// </summary>
        /// <param name="eventName">The collision event name (Events.Collision.Enter or Exit).</param>
        /// <param name="otherGameObject">The other GameObject involved in the collision.</param>
        private void EmitCollisionEvent(string eventName, GameObject otherGameObject)
        {
            if (ownerEntity == null) return;

            // Look up the JS API wrapper for the owning entity
            var ownerPublic = EntityAPIHelper.GetPublicEntity(ownerEntity);
            if (ownerPublic == null) return;

            // Performance guard: only proceed if entity has listeners for this collision event
            // This avoids Jint overhead for entities without collision handlers
            if (!ownerPublic.Listeners.ContainsKey(eventName)) return;

            // Find the other entity involved in the collision
            var otherInternal = otherGameObject.GetComponentInParent<BaseEntity>();
            Jint.Native.JsValue otherJsValue = Jint.Native.JsValue.Null;

            if (otherInternal != null)
            {
                var otherPublic = EntityAPIHelper.GetPublicEntity(otherInternal);
                if (otherPublic != null)
                {
                    try
                    {
                        var engine = WebVerseRuntime.Instance?.javascriptHandler?.Engine;
                        if (engine != null)
                        {
                            otherJsValue = Jint.Native.JsValue.FromObject(engine, otherPublic);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Logging.LogError(
                            $"[EventSystem] Failed to convert other entity to JsValue: {ex.Message}");
                    }
                }
            }

            // Emit the collision event on the owning entity
            try
            {
                ((IEventEmitter)ownerPublic).Emit(eventName, otherJsValue);
            }
            catch (System.Exception ex)
            {
                Logging.LogError(
                    $"[EventSystem] Collision emit error for '{eventName}': {ex.Message}");
            }
        }
    }
}
