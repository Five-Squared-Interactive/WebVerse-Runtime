// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Component that makes a GameObject always face the active camera.
    /// Primarily used for character labels and UI elements that need to remain readable
    /// regardless of camera position or orientation.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        /// <summary>
        /// Whether to constrain the billboard rotation to only the Y axis.
        /// When true, the billboard will only rotate horizontally.
        /// </summary>
        [Tooltip("Whether to constrain the billboard rotation to only the Y axis.")]
        public bool lockXAxis = false;

        /// <summary>
        /// The camera to face. If null, will automatically find the main camera.
        /// </summary>
        private UnityEngine.Camera targetCamera;

        void Start()
        {
            // Find the target camera
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
                if (targetCamera == null)
                {
                    targetCamera = FindObjectOfType<UnityEngine.Camera>();
                }
            }
        }

        void Update()
        {
            // Ensure we have a valid camera to face
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
                if (targetCamera == null)
                {
                    targetCamera = FindObjectOfType<UnityEngine.Camera>();
                }
                
                if (targetCamera == null)
                {
                    return; // No camera found, can't billboard
                }
            }

            // Calculate the direction to face the camera
            Vector3 targetPosition = targetCamera.transform.position;
            Vector3 direction = targetPosition - transform.position;

            // If lockXAxis is true, only rotate around the Y axis
            if (lockXAxis)
            {
                direction.y = 0;
            }

            // Only proceed if we have a valid direction
            if (direction.magnitude > 0.001f)
            {
                // Calculate the rotation to face the camera
                Quaternion targetRotation = Quaternion.LookRotation(Quaternion.Euler(0, 180, 0) * direction);
                
                // Apply the rotation
                transform.rotation = targetRotation;
            }
        }

        /// <summary>
        /// Manually set the target camera for this billboard.
        /// </summary>
        /// <param name="camera">The camera to face.</param>
        public void SetTargetCamera(UnityEngine.Camera camera)
        {
            targetCamera = camera;
        }
    }
}