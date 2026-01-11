// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.PhysicsGravity;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_physics_gravity.
    /// Creates gravity volume triggers with custom gravity behavior.
    /// </summary>
    public class StraightFourPhysicsGravityNodeHandler : StraightFourNodeHandlerBase<OMIPhysicsGravityNode>
    {
        public override string ExtensionName => OMIPhysicsGravityExtension.ExtensionName;
        public override int Priority => 80;

        public override Task OnNodeImportAsync(OMIPhysicsGravityNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null)
            {
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing gravity volume for node {nodeIndex}: type={data.Type}");

            // Add gravity volume component
            var gravityVolume = targetObject.AddComponent<OMIGravityVolume>();
            gravityVolume.Initialize(data);

            // Ensure there's a trigger collider
            var collider = targetObject.GetComponent<Collider>();
            if (collider == null)
            {
                // Add default box collider
                var boxCollider = targetObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
            }
            else
            {
                collider.isTrigger = true;
            }

            Logging.Log($"[StraightFour] Created gravity volume: type={data.Type}, gravity={data.Gravity}");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Behavior component for gravity volumes.
    /// </summary>
    public class OMIGravityVolume : MonoBehaviour
    {
        public string Type { get; private set; }
        public float Gravity { get; private set; }
        public int Priority { get; private set; }
        public bool Replace { get; private set; }
        public bool Stop { get; private set; }
        public Vector3 Direction { get; private set; }
        public float UnitDistance { get; private set; }
        public float Radius { get; private set; }

        private OMIPhysicsGravityNode nodeData;

        public void Initialize(OMIPhysicsGravityNode data)
        {
            nodeData = data;
            Type = data.Type;
            Gravity = data.Gravity;
            Priority = data.Priority;
            Replace = data.Replace;
            Stop = data.Stop;

            // Extract type-specific parameters
            switch (Type)
            {
                case OMIGravityType.Directional:
                    if (data.Directional?.Direction != null && data.Directional.Direction.Length >= 3)
                    {
                        Direction = new Vector3(data.Directional.Direction[0], data.Directional.Direction[1], data.Directional.Direction[2]).normalized;
                    }
                    else
                    {
                        Direction = Vector3.down;
                    }
                    break;

                case OMIGravityType.Point:
                    UnitDistance = data.Point?.UnitDistance ?? 0f;
                    break;

                case OMIGravityType.Disc:
                    Radius = data.Disc?.Radius ?? 1f;
                    UnitDistance = data.Disc?.UnitDistance ?? 0f;
                    break;

                case OMIGravityType.Torus:
                    Radius = data.Torus?.Radius ?? 1f;
                    UnitDistance = data.Torus?.UnitDistance ?? 0f;
                    break;

                case OMIGravityType.Line:
                    UnitDistance = data.Line?.UnitDistance ?? 0f;
                    break;
            }
        }

        /// <summary>
        /// Calculate gravity for a position within this volume.
        /// </summary>
        public Vector3 CalculateGravity(Vector3 worldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            switch (Type)
            {
                case OMIGravityType.Directional:
                    return transform.TransformDirection(Direction) * Gravity;

                case OMIGravityType.Point:
                    Vector3 toCenter = -localPos.normalized;
                    float distance = localPos.magnitude;
                    float gravityMagnitude = CalculateFalloff(distance);
                    return transform.TransformDirection(toCenter) * gravityMagnitude;

                case OMIGravityType.Disc:
                    // Attract to nearest point on XZ plane disc
                    Vector3 discPoint = new Vector3(
                        Mathf.Clamp(localPos.x, -Radius, Radius),
                        0,
                        Mathf.Clamp(localPos.z, -Radius, Radius));
                    Vector3 toDisc = (discPoint - localPos).normalized;
                    float discDistance = (localPos - discPoint).magnitude;
                    return transform.TransformDirection(toDisc) * CalculateFalloff(discDistance);

                case OMIGravityType.Torus:
                    // Attract to nearest point on torus ring
                    Vector3 xzPos = new Vector3(localPos.x, 0, localPos.z);
                    Vector3 ringPoint = xzPos.normalized * Radius;
                    Vector3 toRing = (ringPoint - localPos).normalized;
                    float ringDistance = (localPos - ringPoint).magnitude;
                    return transform.TransformDirection(toRing) * CalculateFalloff(ringDistance);

                case OMIGravityType.Line:
                    // Attract to Y axis (line)
                    Vector3 linePoint = new Vector3(0, localPos.y, 0);
                    Vector3 toLine = (linePoint - localPos).normalized;
                    float lineDistance = new Vector2(localPos.x, localPos.z).magnitude;
                    return transform.TransformDirection(toLine) * CalculateFalloff(lineDistance);

                default:
                    return Physics.gravity;
            }
        }

        private float CalculateFalloff(float distance)
        {
            if (UnitDistance <= 0)
            {
                return Gravity;
            }
            // Inverse square law: g = G * (unitDistance / distance)^2
            float ratio = UnitDistance / Mathf.Max(distance, 0.001f);
            return Gravity * ratio * ratio;
        }

        private void OnTriggerStay(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null || rb.isKinematic)
                return;

            // Apply gravity
            Vector3 gravity = CalculateGravity(rb.position);

            if (Replace)
            {
                // Replace existing gravity
                rb.useGravity = false;
            }

            rb.AddForce(gravity, ForceMode.Acceleration);
        }

        private void OnTriggerExit(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb != null && Replace)
            {
                // Restore normal gravity
                rb.useGravity = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;

            switch (Type)
            {
                case OMIGravityType.Point:
                    Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
                    break;
                case OMIGravityType.Disc:
                    Gizmos.DrawWireSphere(Vector3.zero, Radius);
                    break;
                case OMIGravityType.Torus:
                    // Draw torus approximation
                    for (int i = 0; i < 32; i++)
                    {
                        float angle = i * Mathf.PI * 2 / 32;
                        Vector3 point = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * Radius;
                        Gizmos.DrawWireSphere(point, 0.1f);
                    }
                    break;
                case OMIGravityType.Line:
                    Gizmos.DrawLine(Vector3.up * 5, Vector3.down * 5);
                    break;
                default:
                    Gizmos.DrawLine(Vector3.zero, Direction * 2);
                    break;
            }
        }
    }
}
#endif
