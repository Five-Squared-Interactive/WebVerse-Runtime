// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Applies buoyancy forces to registered rigidbodies each physics step, and tracks player
    /// submersion state. Cost scales with the number of registered bodies, not water area.
    /// Lazily created singleton.
    /// </summary>
    public class BuoyancyController : MonoBehaviour
    {
        private class BuoyantBody
        {
            public Rigidbody rigidBody;
            public BuoyancyProfile profile;
            public float baseLinearDamping;
            public float baseAngularDamping;
        }

        private static BuoyancyController instance;

        /// <summary>
        /// The active controller, created on first access.
        /// </summary>
        public static BuoyancyController Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("BuoyancyController");
                    instance = go.AddComponent<BuoyancyController>();
                }
                return instance;
            }
        }

        private readonly List<BuoyantBody> bodies = new List<BuoyantBody>();
        private readonly Dictionary<Rigidbody, BuoyantBody> bodyLookup = new Dictionary<Rigidbody, BuoyantBody>();
        private readonly List<WaterBodyEntity> waters = new List<WaterBodyEntity>();
        private readonly List<WaterVolume> volumeScratch = new List<WaterVolume>();
        private readonly UnderwaterStateTracker playerTracker = new UnderwaterStateTracker();

        /// <summary>
        /// Optional point (e.g. the player camera/rig) tracked for submersion enter/exit events.
        /// </summary>
        public Transform playerPoint;

        /// <summary>
        /// Raised when the tracked player point enters (true) or exits (false) water.
        /// </summary>
        public event Action<bool> OnPlayerUnderwaterChanged;

        /// <summary>Number of registered buoyant bodies.</summary>
        public int BodyCount { get { return bodies.Count; } }

        /// <summary>Whether a controller instance currently exists (does not create one).</summary>
        public static bool HasInstance { get { return instance != null; } }

        /// <summary>
        /// Destroy the singleton and clear state. For test isolation.
        /// </summary>
        public static void ResetForTests()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                instance = null;
            }
        }

        /// <summary>
        /// Register a water body so its volume participates in buoyancy and submersion.
        /// </summary>
        public void RegisterWater(WaterBodyEntity water)
        {
            if (water != null && !waters.Contains(water))
            {
                waters.Add(water);
            }
        }

        /// <summary>
        /// Stop a water body from participating.
        /// </summary>
        public void UnregisterWater(WaterBodyEntity water)
        {
            waters.Remove(water);
        }

        /// <summary>
        /// Register (opt-in) a rigidbody to receive buoyancy. Re-registering updates the profile.
        /// </summary>
        public void RegisterBody(Rigidbody rigidBody, BuoyancyProfile profile)
        {
            if (rigidBody == null)
            {
                return;
            }
            if (bodyLookup.TryGetValue(rigidBody, out BuoyantBody existing))
            {
                existing.profile = profile;
                return;
            }
            BuoyantBody body = new BuoyantBody
            {
                rigidBody = rigidBody,
                profile = profile,
                baseLinearDamping = rigidBody.linearDamping,
                baseAngularDamping = rigidBody.angularDamping
            };
            bodies.Add(body);
            bodyLookup[rigidBody] = body;
        }

        /// <summary>
        /// Unregister a rigidbody and restore its original damping.
        /// </summary>
        public void UnregisterBody(Rigidbody rigidBody)
        {
            if (rigidBody != null && bodyLookup.TryGetValue(rigidBody, out BuoyantBody body))
            {
                rigidBody.linearDamping = body.baseLinearDamping;
                rigidBody.angularDamping = body.baseAngularDamping;
                bodies.Remove(body);
                bodyLookup.Remove(rigidBody);
            }
        }

        /// <summary>
        /// Whether a rigidbody is registered for buoyancy.
        /// </summary>
        public bool IsRegistered(Rigidbody rigidBody)
        {
            return rigidBody != null && bodyLookup.ContainsKey(rigidBody);
        }

        private void FixedUpdate()
        {
            // Compact entries whose Unity objects were destroyed without an explicit unregister
            // (e.g. an entity deleted on world unload). Prevents the lists growing unbounded.
            for (int i = bodies.Count - 1; i >= 0; i--)
            {
                if (bodies[i].rigidBody == null)
                {
                    bodyLookup.Remove(bodies[i].rigidBody);
                    bodies.RemoveAt(i);
                }
            }
            for (int i = waters.Count - 1; i >= 0; i--)
            {
                if (waters[i] == null)
                {
                    waters.RemoveAt(i);
                }
            }

            volumeScratch.Clear();
            for (int i = 0; i < waters.Count; i++)
            {
                WaterBodyEntity w = waters[i];
                if (w != null && w.HasWaterVolume())
                {
                    volumeScratch.Add(w.GetWaterVolume());
                }
            }

            for (int i = 0; i < bodies.Count; i++)
            {
                BuoyantBody body = bodies[i];
                if (body.rigidBody == null)
                {
                    continue;
                }

                Vector3 com = body.rigidBody.worldCenterOfMass;
                bool submerged = false;
                if (Buoyancy.SelectGoverningVolume(com, volumeScratch, out int idx))
                {
                    // Sample the wavy surface (matches the rendered Gerstner waves) so floating
                    // bodies bob. shaderTime mirrors Unity's _Time.x (Time.time / 20).
                    // Match the URPWater shader's _Time.x, which is timeSinceLevelLoad / 20,
                    // so the physics surface stays in phase with the rendered crests.
                    float shaderTime = Time.timeSinceLevelLoad / 20f;
                    float depth = volumeScratch[idx].SubmergedDepth(com, shaderTime);
                    if (depth > 0f)
                    {
                        body.rigidBody.AddForce(Vector3.up * Buoyancy.UpwardForce(depth, body.profile), ForceMode.Force);

                        // Horizontal shove from the wave slope: water pushes the body downhill
                        // (opposite the surface gradient), so passing crests move it along.
                        if (body.profile.lateralStrength > 0f)
                        {
                            Vector2 slope = volumeScratch[idx].waves.SurfaceSlope(com.x, com.z, shaderTime);
                            Vector3 lateral = new Vector3(-slope.x, 0f, -slope.y) * (body.profile.lateralStrength * depth);
                            lateral = Vector3.ClampMagnitude(lateral, body.profile.lateralStrength * 3f);
                            body.rigidBody.AddForce(lateral, ForceMode.Force);
                        }

                        body.rigidBody.linearDamping = body.profile.submergedLinearDrag;
                        body.rigidBody.angularDamping = body.profile.submergedAngularDrag;
                        submerged = true;
                    }
                }

                if (!submerged)
                {
                    body.rigidBody.linearDamping = body.baseLinearDamping;
                    body.rigidBody.angularDamping = body.baseAngularDamping;
                }
            }

            UpdatePlayerSubmersion();
        }

        private void UpdatePlayerSubmersion()
        {
            if (playerPoint == null)
            {
                return;
            }

            Vector3 p = playerPoint.position;
            float signedDepth = -9999f;
            if (Buoyancy.SelectGoverningVolume(p, volumeScratch, out int idx))
            {
                signedDepth = volumeScratch[idx].waterLevel - p.y;
            }

            UnderwaterStateTracker.Transition transition = playerTracker.Update(signedDepth);
            if (transition != UnderwaterStateTracker.Transition.None)
            {
                OnPlayerUnderwaterChanged?.Invoke(playerTracker.IsUnderwater);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
