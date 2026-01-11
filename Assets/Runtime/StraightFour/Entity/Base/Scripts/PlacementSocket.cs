// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Entity.Placement
{
    /// <summary>
    /// A Generic socket for supporting the placement of entities.
    /// </summary>
    public class PlacementSocket : MonoBehaviour
    {
        /// <summary>
        /// Position of the socket.
        /// </summary>
        public Vector3 position
        {
            get { return transform.localPosition; }
        }

        /// <summary>
        /// Rotation of the socket.
        /// </summary>
        public Quaternion rotation
        {
            get { return transform.localRotation; }
        }

        /// <summary>
        /// Entity that this socket belongs to.
        /// </summary>
        public BaseEntity entity { get; private set; }

        /// <summary>
        /// Position to maintain during snapping.
        /// </summary>
        private Vector3? positionToKeep = null;

        /// <summary>
        /// Rotation to maintain during snapping.
        /// </summary>
        private Quaternion? rotationToKeep = null;

        /// <summary>
        /// Socket that is currently being interacted with.
        /// </summary>
        private PlacementSocket interactingSocket = null;

        /// <summary>
        /// Offset to apply when connecting to another socket.
        /// </summary>
        private Vector3 connectingOffset = Vector3.zero;

        /// <summary>
        /// Initialize the placement socket.
        /// </summary>
        /// <param name="entity">Entity which the socket belongs to.</param>
        /// <param name="position">Position of the socket.</param>
        /// <param name="rotation">Rotation of the socket.</param>
        /// <param name="connectingOffset">Offset to apply when connecting to another socket.</param>
        public void Initialize(BaseEntity entity, Vector3 position, Quaternion rotation, Vector3 connectingOffset)
        {
            this.entity = entity;
            transform.localPosition = position;
            transform.localRotation = rotation;
            BoxCollider bc = gameObject.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            this.connectingOffset = connectingOffset;
        }

        /// <summary>
        /// Handle trigger collision.
        /// </summary>
        /// <param name="other">Collider collided with.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (entity == null)
            {
                return;
            }
            
            if (entity.GetInteractionState() != BaseEntity.InteractionState.Placing)
            {
                return;
            }
            
            PlacementSocket otherSocket = other.GetComponent<PlacementSocket>();
            if (otherSocket != null)
            {
                InteractWithOtherSocket(otherSocket);
            }
        }

        /// <summary>
        /// Handle end of trigger collision.
        /// </summary>
        /// <param name="other">Collider uncollided with.</param>
        private void OnTriggerExit(Collider other)
        {
            if (entity == null)
            {
                return;
            }

            if (entity.GetInteractionState() != BaseEntity.InteractionState.Placing)
            {
                return;
            }

            PlacementSocket otherSocket = other.GetComponent<PlacementSocket>();
            if (otherSocket != null)
            {
                if (otherSocket == interactingSocket)
                {
                    interactingSocket = null;
                    positionToKeep = null;
                    rotationToKeep = null;
                    entity.ResetPreview();
                }
            }
        }

        /// <summary>
        /// Interact with another socket.
        /// </summary>
        /// <param name="otherSocket">Socket to interact with.</param>
        private void InteractWithOtherSocket(PlacementSocket otherSocket)
        {
            BaseEntity otherEntity = otherSocket.entity;
            if (otherEntity == null)
            {
                return;
            }

            // Ignore if socket belongs to this entity.
            if (otherEntity == entity)
            {
                return;
            }

            GameObject sockLocator = new GameObject();
            sockLocator.transform.SetParent(otherSocket.transform);
            sockLocator.transform.localPosition = connectingOffset;
            sockLocator.transform.localEulerAngles = new Vector3(0, 180, 0);
            sockLocator.transform.SetParent(null);

            GameObject previewLocator = new GameObject();
            previewLocator.transform.SetParent(sockLocator.transform);
            Transform origEntityParent = entity.transform.parent;
            Transform origSocketParent = transform.parent;
            transform.SetParent(null);
            entity.transform.SetParent(transform);
            previewLocator.transform.localPosition = entity.transform.localPosition;
            previewLocator.transform.localRotation = entity.transform.localRotation;

            entity.SnapPreview(previewLocator.transform.position, previewLocator.transform.rotation);
            entity.transform.SetParent(null);
            transform.SetParent(origSocketParent);
            entity.transform.SetParent(origEntityParent);
            Destroy(previewLocator);
            Destroy(sockLocator);

            interactingSocket = otherSocket;
            positionToKeep = previewLocator.transform.position;
            rotationToKeep = previewLocator.transform.rotation;
        }

        private void Update()
        {
            if (positionToKeep.HasValue && rotationToKeep.HasValue)
            {
                entity.SnapPreview(positionToKeep.Value, rotationToKeep.Value);
            }
        }
    }
}