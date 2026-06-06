// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Manages avatar animation lifecycle, emotes, and tracking mode.
    /// Handles default avatar instantiation and Animator controller setup.
    /// </summary>
    public class AvatarAnimationManager : MonoBehaviour
    {
        /// <summary>
        /// Global default avatar mode. Set by the runtime from user settings.
        /// "rigged" loads the animated mannequin, "simple" keeps original entity renderers.
        /// </summary>
        public static string DefaultAvatarMode = "rigged";

        /// <summary>
        /// Fired when an avatar model is successfully loaded and configured.
        /// Parameter: avatar model URI.
        /// </summary>
        public event Action<string> OnAvatarLoaded;

        /// <summary>
        /// Fired when an avatar model fails to load.
        /// Parameter: error message.
        /// </summary>
        public event Action<string> OnAvatarLoadFailed;

        /// <summary>
        /// Fired when an emote animation starts playing.
        /// Parameter: emote name.
        /// </summary>
        public event Action<string> OnEmoteStarted;

        /// <summary>
        /// Fired when an emote animation finishes playing.
        /// Parameter: emote name.
        /// </summary>
        public event Action<string> OnEmoteEnded;

        // Remaining events invoked in future stories (tracking: Epic 3).
#pragma warning disable CS0067
        /// <summary>
        /// Fired when the tracking mode changes between Animation and IK.
        /// Parameter: new tracking mode.
        /// </summary>
        public event Action<AvatarTrackingMode> OnTrackingModeChanged;
#pragma warning restore CS0067

        private Animator _animator;
        private bool _isInitialized;
        private GameObject _defaultAvatarInstance;
        private Renderer[] _originalRenderers;
        private AvatarLoader _avatarLoader;
        private AvatarNotificationDisplay _notificationDisplay;
        private AvatarLocomotionDriver _locomotionDriver;
        private AvatarHeadTrackingDriver _headTrackingDriver;
        private AvatarEmoteDriver _emoteDriver;

        /// <summary>
        /// Whether the avatar animation system has been initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// The Animator component managed by this manager.
        /// Read-only access for AvatarRigController in future stories.
        /// </summary>
        public Animator Animator => _animator;

        /// <summary>
        /// The AvatarLoader component for loading custom avatars.
        /// </summary>
        public AvatarLoader AvatarLoader => _avatarLoader;

        /// <summary>
        /// The AvatarLocomotionDriver component for driving locomotion blend tree.
        /// </summary>
        public AvatarLocomotionDriver LocomotionDriver => _locomotionDriver;

        /// <summary>
        /// The AvatarHeadTrackingDriver component for driving head bone rotation from mouse-look.
        /// </summary>
        public AvatarHeadTrackingDriver HeadTrackingDriver => _headTrackingDriver;

        /// <summary>
        /// The AvatarEmoteDriver component for playing emote animations.
        /// </summary>
        public AvatarEmoteDriver EmoteDriver => _emoteDriver;

        /// <summary>
        /// Initializes the avatar animation system.
        /// Gets or creates an Animator, loads the controller from Resources,
        /// and instantiates the default avatar.
        /// </summary>
        /// <param name="avatarMode">Avatar mode: "rigged" loads the rigged mannequin,
        /// "simple" keeps the original entity renderers.</param>
        public void Initialize(string avatarMode = "rigged")
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<Animator>();
            }

            if (avatarMode != "simple")
            {
                var controller = Resources.Load<RuntimeAnimatorController>("AvatarAnimatorController");
                if (controller != null)
                {
                    _animator.runtimeAnimatorController = controller;
                }
                else
                {
                    Debug.LogWarning("[AvatarAnimationManager] AvatarAnimatorController not found in Resources");
                }

                LoadDefaultAvatar();
            }
            else
            {
                Debug.Log("[AvatarAnimationManager] Simple avatar mode — using original entity renderers");
            }

            // Create AvatarLoader for custom avatar loading
            _avatarLoader = gameObject.GetComponent<AvatarLoader>();
            if (_avatarLoader == null)
            {
                _avatarLoader = gameObject.AddComponent<AvatarLoader>();
            }
            _avatarLoader.Initialize(this);

            // Create or find notification display for user-facing error messages
            _notificationDisplay = gameObject.GetComponent<AvatarNotificationDisplay>();
            if (_notificationDisplay == null)
            {
                _notificationDisplay = gameObject.AddComponent<AvatarNotificationDisplay>();
            }

            // Create or find locomotion driver for blend tree parameters
            _locomotionDriver = gameObject.GetComponent<AvatarLocomotionDriver>();
            if (_locomotionDriver == null)
            {
                _locomotionDriver = gameObject.AddComponent<AvatarLocomotionDriver>();
            }
            _locomotionDriver.Initialize(this);

            // Create or find head tracking driver for mouse-look head rotation
            _headTrackingDriver = gameObject.GetComponent<AvatarHeadTrackingDriver>();
            if (_headTrackingDriver == null)
            {
                _headTrackingDriver = gameObject.AddComponent<AvatarHeadTrackingDriver>();
            }
            _headTrackingDriver.Initialize(this);

            // Create or find emote driver for emote animation triggers
            _emoteDriver = gameObject.GetComponent<AvatarEmoteDriver>();
            if (_emoteDriver == null)
            {
                _emoteDriver = gameObject.AddComponent<AvatarEmoteDriver>();
            }
            _emoteDriver.Initialize(this);

            _isInitialized = true;
            Debug.Log("[AvatarAnimationManager] Initialized");
        }

        /// <summary>
        /// Cleans up avatar animation resources.
        /// Destroys the default avatar instance and re-enables original renderers.
        /// </summary>
        public void Cleanup()
        {
            if (_avatarLoader != null)
            {
                _avatarLoader.Cleanup();
            }

            if (_animator != null)
            {
                _animator.runtimeAnimatorController = null;
            }

            if (_defaultAvatarInstance != null)
            {
                Destroy(_defaultAvatarInstance);
                _defaultAvatarInstance = null;
            }

            // Disable locomotion driver
            if (_locomotionDriver != null)
            {
                _locomotionDriver.enabled = false;
            }

            // Disable head tracking driver
            if (_headTrackingDriver != null)
            {
                _headTrackingDriver.enabled = false;
            }

            // Stop and disable emote driver
            if (_emoteDriver != null)
            {
                _emoteDriver.StopEmote();
                _emoteDriver.enabled = false;
            }

            // Restore entity-level Animator reference (prefab Animator was destroyed above)
            _animator = GetComponent<Animator>();

            // Re-enable original renderers if they were disabled
            if (_originalRenderers != null)
            {
                foreach (var renderer in _originalRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                    }
                }
                _originalRenderers = null;
            }

            _isInitialized = false;
            Debug.Log("[AvatarAnimationManager] Cleaned up");
        }

        /// <summary>
        /// Loads and instantiates the default avatar prefab as a child of this entity.
        /// Disables existing mesh renderers to avoid doubling visuals.
        /// </summary>
        public void LoadDefaultAvatar()
        {
            // Store and disable existing renderers (UserAvatar primitive meshes)
            _originalRenderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in _originalRenderers)
            {
                renderer.enabled = false;
            }

            // Load and instantiate default avatar
            var prefab = Resources.Load<GameObject>("DefaultAvatar");
            if (prefab != null)
            {
                _defaultAvatarInstance = Instantiate(prefab, transform);
                _defaultAvatarInstance.name = "DefaultAvatar";
                _defaultAvatarInstance.transform.localRotation = Quaternion.identity;

                // Offset avatar down so feet touch the ground.
                // CharacterController places the entity root at center.y + height/2 above ground.
                // The avatar's feet are near y=0.10 in local space, so we shift down accordingly.
                var cc = GetComponent<CharacterController>();
                if (cc != null)
                {
                    float yOffset = -(cc.height / 2f + cc.center.y);
                    _defaultAvatarInstance.transform.localPosition = new Vector3(0, yOffset, 0);
                }
                else
                {
                    _defaultAvatarInstance.transform.localPosition = Vector3.zero;
                }

                // Apply colorful materials matching the non-rigged avatar style
                ApplyDefaultAvatarMaterials(_defaultAvatarInstance);

                // The prefab's Animator must drive animation because the skeleton
                // bones are children of the prefab root, not the entity root.
                // Transfer the controller to the prefab's Animator and use it as primary.
                var prefabAnimator = _defaultAvatarInstance.GetComponent<Animator>();
                if (prefabAnimator != null)
                {
                    if (_animator != null)
                    {
                        prefabAnimator.runtimeAnimatorController = _animator.runtimeAnimatorController;
                        // Clear entity-level Animator so it doesn't conflict
                        _animator.runtimeAnimatorController = null;
                    }
                    _animator = prefabAnimator;
                }
                else
                {
                    // No Animator on prefab — keep using entity-level Animator
                    Debug.LogWarning("[AvatarAnimationManager] DefaultAvatar prefab has no Animator component");
                }

                OnAvatarLoaded?.Invoke("default");
                Debug.Log("[AvatarAnimationManager] Default avatar loaded");
            }
            else
            {
                Debug.LogWarning("[AvatarAnimationManager] DefaultAvatar prefab not found in Resources");
                // Re-enable original renderers as fallback
                if (_originalRenderers != null)
                {
                    foreach (var renderer in _originalRenderers)
                    {
                        if (renderer != null)
                        {
                            renderer.enabled = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Destroys the current avatar (default or custom) and re-enables original renderers.
        /// Called by AvatarLoader before applying a new custom avatar.
        /// </summary>
        public void DestroyCurrentAvatar()
        {
            if (_defaultAvatarInstance != null)
            {
                Destroy(_defaultAvatarInstance);
                _defaultAvatarInstance = null;
            }

            // Restore entity-level Animator reference
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<Animator>();
            }
        }

        /// <summary>
        /// Sets the active Animator reference. Called by AvatarLoader after
        /// configuring the custom avatar's Animator.
        /// </summary>
        /// <param name="animator">The new primary Animator.</param>
        public void SetAnimator(Animator animator)
        {
            // Clear entity-level animator controller to avoid conflicts
            var entityAnimator = GetComponent<Animator>();
            if (entityAnimator != null && entityAnimator != animator)
            {
                entityAnimator.runtimeAnimatorController = null;
            }
            _animator = animator;

            // Update locomotion driver's cached Animator reference
            if (_locomotionDriver != null)
            {
                _locomotionDriver.UpdateAnimator(animator);
            }

            // Update head tracking driver's cached Animator and head bone reference
            if (_headTrackingDriver != null)
            {
                _headTrackingDriver.UpdateAnimator(animator);
            }

            // Update emote driver's cached Animator reference
            if (_emoteDriver != null)
            {
                _emoteDriver.UpdateAnimator(animator);
            }
        }

        /// <summary>
        /// Fires the OnAvatarLoaded event. Called by AvatarLoader on successful load.
        /// </summary>
        /// <param name="uri">The avatar model URI.</param>
        internal void FireAvatarLoaded(string uri)
        {
            OnAvatarLoaded?.Invoke(uri);
        }

        /// <summary>
        /// Fires the OnAvatarLoadFailed event. Called by AvatarLoader on load failure.
        /// </summary>
        /// <param name="message">Error message describing the failure.</param>
        internal void FireAvatarLoadFailed(string message)
        {
            OnAvatarLoadFailed?.Invoke(message);
        }

        /// <summary>
        /// Fires the OnEmoteStarted event. Called by AvatarEmoteDriver when an emote begins.
        /// </summary>
        /// <param name="emoteName">The name of the emote that started.</param>
        internal void FireEmoteStarted(string emoteName)
        {
            OnEmoteStarted?.Invoke(emoteName);
        }

        /// <summary>
        /// Fires the OnEmoteEnded event. Called by AvatarEmoteDriver when an emote ends.
        /// </summary>
        /// <param name="emoteName">The name of the emote that ended.</param>
        internal void FireEmoteEnded(string emoteName)
        {
            OnEmoteEnded?.Invoke(emoteName);
        }

        /// <summary>
        /// Shows a user-friendly notification via the notification display.
        /// Called by AvatarLoader on load failures.
        /// </summary>
        /// <param name="userMessage">User-friendly message to display.</param>
        internal void ShowNotification(string userMessage)
        {
            if (_notificationDisplay != null)
            {
                _notificationDisplay.Show(userMessage);
            }
            else
            {
                Debug.Log($"[AvatarAnimationManager] Notification: {userMessage}");
            }
        }

        /// <summary>
        /// Applies colorful materials to the default avatar's visual meshes,
        /// matching the non-rigged avatar's color scheme (cyan body, green head).
        /// </summary>
        private void ApplyDefaultAvatarMaterials(GameObject avatar)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogWarning("[AvatarAnimationManager] URP Lit shader not found for avatar materials");
                return;
            }

            var torsoMat = new Material(shader);
            torsoMat.SetColor("_BaseColor", new Color(0f, 1f, 0.99f, 1f)); // cyan
            torsoMat.SetColor("_Color", new Color(0f, 1f, 0.99f, 1f));

            var headMat = new Material(shader);
            headMat.SetColor("_BaseColor", new Color(0f, 1f, 0.67f, 1f)); // green
            headMat.SetColor("_Color", new Color(0f, 1f, 0.67f, 1f));

            var eyeMat = new Material(shader);
            eyeMat.SetColor("_BaseColor", new Color(0f, 0f, 0f, 1f)); // black
            eyeMat.SetColor("_Color", new Color(0f, 0f, 0f, 1f));

            Transform headVisual = null;
            foreach (var renderer in avatar.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.gameObject.name.StartsWith("Head", StringComparison.Ordinal))
                {
                    renderer.sharedMaterial = headMat;
                    headVisual = renderer.transform;
                }
                else
                {
                    renderer.sharedMaterial = torsoMat;
                }
            }

            // Add eyes as small black cubes on the front face of the head
            if (headVisual != null)
            {
                CreateEye("LeftEye", headVisual.parent, new Vector3(-0.04f, 0.12f, 0.09f), eyeMat);
                CreateEye("RightEye", headVisual.parent, new Vector3(0.04f, 0.12f, 0.09f), eyeMat);
            }
        }

        private void CreateEye(string name, Transform parent, Vector3 localPos, Material mat)
        {
            var eye = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eye.name = name;
            Destroy(eye.GetComponent<BoxCollider>());
            eye.transform.SetParent(parent, false);
            eye.transform.localPosition = localPos;
            eye.transform.localScale = new Vector3(0.04f, 0.04f, 0.02f);
            eye.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        /// <summary>
        /// Builds a Humanoid Avatar at runtime from the model's bone hierarchy
        /// and assigns it to the Animator. Falls back to Generic if building fails.
        /// </summary>
        private void TryBuildHumanoidAvatar(GameObject model, Animator animator)
        {
            try
            {
                var humanBones = BuildHumanBoneArray(model.transform);
                if (humanBones == null || humanBones.Length == 0)
                {
                    Debug.LogWarning("[AvatarAnimationManager] Could not map bones for Humanoid Avatar");
                    return;
                }

                var skeletonBones = BuildSkeletonBoneArray(model.transform);

                var description = new HumanDescription
                {
                    human = humanBones,
                    skeleton = skeletonBones,
                    upperArmTwist = 0.5f,
                    lowerArmTwist = 0.5f,
                    upperLegTwist = 0.5f,
                    lowerLegTwist = 0.5f,
                    armStretch = 0.05f,
                    legStretch = 0.05f,
                    feetSpacing = 0f,
                    hasTranslationDoF = false
                };

                var avatar = AvatarBuilder.BuildHumanAvatar(model, description);
                if (avatar != null && avatar.isValid && avatar.isHuman)
                {
                    animator.avatar = avatar;
                    Debug.Log("[AvatarAnimationManager] Built Humanoid Avatar for default avatar");
                }
                else
                {
                    Debug.LogWarning("[AvatarAnimationManager] AvatarBuilder produced invalid result");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AvatarAnimationManager] Failed to build Humanoid Avatar: {ex.Message}");
            }
        }

        private HumanBone[] BuildHumanBoneArray(Transform root)
        {
            // Keys = Transform names in the prefab hierarchy,
            // Values = Mecanim humanoid bone names (must match Unity's exact strings with spaces).
            var boneMapping = new Dictionary<string, string>
            {
                { "Hips", "Hips" }, { "Spine", "Spine" }, { "Chest", "Chest" },
                { "Head", "Head" },
                { "LeftUpperArm", "Left Upper Arm" }, { "LeftLowerArm", "Left Lower Arm" },
                { "LeftHand", "Left Hand" },
                { "RightUpperArm", "Right Upper Arm" }, { "RightLowerArm", "Right Lower Arm" },
                { "RightHand", "Right Hand" },
                { "LeftUpperLeg", "Left Upper Leg" }, { "LeftLowerLeg", "Left Lower Leg" },
                { "LeftFoot", "Left Foot" },
                { "RightUpperLeg", "Right Upper Leg" }, { "RightLowerLeg", "Right Lower Leg" },
                { "RightFoot", "Right Foot" }
            };

            var optionalMapping = new Dictionary<string, string>
            {
                { "UpperChest", "Upper Chest" },
                { "Neck", "Neck" }, { "LeftShoulder", "Left Shoulder" },
                { "RightShoulder", "Right Shoulder" }
            };

            var nameToTransform = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            CollectTransformNames(root, nameToTransform);

            var humanBones = new List<HumanBone>();

            foreach (var kvp in boneMapping)
            {
                if (nameToTransform.TryGetValue(kvp.Key, out var actualName))
                {
                    humanBones.Add(new HumanBone
                    {
                        humanName = kvp.Value,
                        boneName = actualName,
                        limit = new HumanLimit { useDefaultValues = true }
                    });
                }
            }

            foreach (var kvp in optionalMapping)
            {
                if (nameToTransform.TryGetValue(kvp.Key, out var actualName))
                {
                    humanBones.Add(new HumanBone
                    {
                        humanName = kvp.Value,
                        boneName = actualName,
                        limit = new HumanLimit { useDefaultValues = true }
                    });
                }
            }

            return humanBones.ToArray();
        }

        private SkeletonBone[] BuildSkeletonBoneArray(Transform root)
        {
            var bones = new List<SkeletonBone>();
            CollectSkeletonBones(root, bones);
            return bones.ToArray();
        }

        private void CollectSkeletonBones(Transform current, List<SkeletonBone> bones)
        {
            // Skip non-skeleton transforms (visual mesh children like "LeftUpperArm_Visual")
            // to avoid polluting the skeleton description with rendering nodes.
            if (current.name.EndsWith("_Visual", StringComparison.Ordinal))
                return;

            bones.Add(new SkeletonBone
            {
                name = current.name,
                position = current.localPosition,
                rotation = current.localRotation,
                scale = current.localScale
            });
            foreach (Transform child in current)
            {
                CollectSkeletonBones(child, bones);
            }
        }

        private void CollectTransformNames(Transform current, Dictionary<string, string> names)
        {
            if (!current.name.EndsWith("_Visual", StringComparison.Ordinal)
                && !names.ContainsKey(current.name))
            {
                names[current.name] = current.name;
            }
            foreach (Transform child in current)
            {
                CollectTransformNames(child, names);
            }
        }
    }
}
