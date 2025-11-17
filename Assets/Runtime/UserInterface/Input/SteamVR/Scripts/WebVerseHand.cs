// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;
using FiveSQD.StraightFour.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.Input.SteamVR
{
    /// <summary>
    /// A class for the WebVerse Hand.
    /// </summary>
    public class WebVerseHand : MonoBehaviour
    {
        /// <summary>
        /// The current entities that are being touched.
        /// </summary>
        public BaseEntity[] touchingEntities
        {
            get
            {
                return touchingEntitiesList.ToArray();
            }
        }

        /// <summary>
        /// The current entities that are being collided with.
        /// </summary>
        public BaseEntity[] collidingEntities
        {
            get
            {
                return collidingEntitiesList.ToArray();
            }
        }

        /// <summary>
        /// Internal list of touching entities.
        /// </summary>
        private List<BaseEntity> touchingEntitiesList = new List<BaseEntity>();

        /// <summary>
        /// Internal list of colliding entities.
        /// </summary>
        private List<BaseEntity> collidingEntitiesList = new List<BaseEntity>();

        /// <summary>
        /// Invoked when a touch occurs.
        /// </summary>
        /// <param name="other">Object being touched.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
            {
                Logging.LogWarning("[WebVerseHand->OnTriggerEnter] No collided entity.");
                return;
            }

            if (other.attachedRigidbody == null)
            {
                Logging.LogWarning("[WebVerseHand->OnTriggerEnter] Collided entity does not contain rigidbody.");
                return;
            }

            BaseEntity touchedEntity = other.GetComponent<BaseEntity>();
            if (touchedEntity == null)
            {
                Logging.LogWarning("[WebVerseHand->OnTriggerEnter] Could not find touched entity.");
                return;
            }

            if (!touchingEntitiesList.Contains(touchedEntity))
            {
                touchingEntitiesList.Add(touchedEntity);
            }
        }

        /// <summary>
        /// Invoked when a touch ends.
        /// </summary>
        /// <param name="other">Object no longer being touched.</param>
        private void OnTriggerExit(Collider other)
        {
            if (other == null)
            {
                Logging.LogWarning("[WebVerseHand->OnTriggerExit] No collided entity.");
                return;
            }

            if (other.attachedRigidbody == null)
            {
                Logging.LogWarning("[WebVerseHand->OnTriggerExit] Collided entity does not contain rigidbody.");
                return;
            }

            BaseEntity touchedEntity = other.GetComponent<BaseEntity>();
            if (touchedEntity == null)
            {
                Logging.LogWarning("[WebVerseHand->OnTriggerExit] Could not find touched entity.");
                return;
            }

            if (touchingEntitiesList.Contains(touchedEntity))
            {
                touchingEntitiesList.Remove(touchedEntity);
            }
        }

        /// <summary>
        /// Invoked when a collision occurs.
        /// </summary>
        /// <param name="collision">Object being collided with.</param>
        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
            {
                Logging.LogWarning("[WebVerseHand->OnCollisionEnter] No collided entity.");
                return;
            }

            if (collision.collider == null)
            {
                Logging.LogWarning("[WebVerseHand->OnCollisionEnter] Collided entity does not contain collider.");
                return;
            }

            if (collision.collider.attachedRigidbody == null)
            {
                Logging.LogWarning("[WebVerseHand->OnCollisionEnter] Collided entity does not contain rigidbody.");
                return;
            }

            BaseEntity collidedEntity = collision.collider.GetComponent<BaseEntity>();
            if (collidedEntity == null)
            {
                Logging.LogWarning("[WebVerseHand->OnCollisionEnter] Could not find collided entity.");
                return;
            }

            if (!collidingEntitiesList.Contains(collidedEntity))
            {
                collidingEntitiesList.Add(collidedEntity);
            }
        }

        /// <summary>
        /// Invoked when a collision ends.
        /// </summary>
        /// <param name="collision">Object no longer being collided with.</param>
        private void OnCollisionExit(Collision collision)
        {
            if (collision == null)
            {
                Logging.LogWarning("[WebVerseHand->OnCollisionExit] No collided entity.");
                return;
            }

            if (collision.collider == null)
            {
                Logging.LogWarning("[WebVerseHand->OnCollisionExit] Collided entity does not contain collider.");
                return;
            }

            if (collision.collider.attachedRigidbody == null)
            {
                Logging.LogWarning("[WebVerseHand->OnCollisionExit] Collided entity does not contain rigidbody.");
                return;
            }

            BaseEntity collidedEntity = collision.collider.GetComponent<BaseEntity>();
            if (collidedEntity == null)
            {
                Logging.LogWarning("[WebVerseHand->OnCollisionExit] Could not find collided entity.");
                return;
            }

            if (collidingEntitiesList.Contains(collidedEntity))
            {
                collidingEntitiesList.Remove(collidedEntity);
            }
        }
    }
}