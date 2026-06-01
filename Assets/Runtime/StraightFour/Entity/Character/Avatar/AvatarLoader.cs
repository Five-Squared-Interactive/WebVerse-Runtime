// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Loads custom avatar models from glTF/VRM URIs, validates their skeletons,
    /// configures Humanoid retargeting, and replaces the current avatar.
    /// </summary>
    public class AvatarLoader : MonoBehaviour
    {
        private AvatarAnimationManager _animationManager;
        private GameObject _currentCustomAvatar;
        private string _currentAvatarUri;

        /// <summary>
        /// URI of the currently loaded custom avatar, or null if using default.
        /// </summary>
        public string CurrentAvatarUri => _currentAvatarUri;

        /// <summary>
        /// Initializes the loader with a reference to the animation manager.
        /// </summary>
        /// <param name="animationManager">The AvatarAnimationManager to integrate with.</param>
        public void Initialize(AvatarAnimationManager animationManager)
        {
            _animationManager = animationManager;
        }

        /// <summary>
        /// Loads a custom avatar from the given URI asynchronously.
        /// On success, replaces the current avatar and fires OnAvatarLoaded.
        /// On failure, falls back to default avatar and fires OnAvatarLoadFailed.
        /// </summary>
        /// <param name="uri">URI to a glTF/GLB/VRM avatar model.</param>
        /// <param name="onComplete">Optional callback: true if load succeeded, false if failed.</param>
        public void LoadAvatarAsync(string uri, Action<bool> onComplete = null)
        {
            if (string.IsNullOrEmpty(uri))
            {
                var errorMsg = "Avatar URI is null or empty";
                Debug.LogWarning($"[AvatarLoader] {errorMsg}");
                DestroyCurrentCustomAvatar();
                _animationManager?.DestroyCurrentAvatar();
                _animationManager?.FireAvatarLoadFailed(errorMsg);
                _animationManager?.ShowNotification(AvatarNotification.MapErrorToUserMessage(errorMsg));
                _animationManager?.LoadDefaultAvatar();
                onComplete?.Invoke(false);
                return;
            }

            StartCoroutine(LoadAvatarCoroutine(uri, onComplete));
        }

        /// <summary>
        /// Coroutine that loads a glTF/VRM model via glTFast.
        /// </summary>
        private IEnumerator LoadAvatarCoroutine(string uri, Action<bool> onComplete)
        {
            GameObject loadedObject = null;
            bool loadFailed = false;
            string errorMessage = null;

            // Create temporary container for loading
            var tempContainer = new GameObject("AvatarLoad_Temp");
            tempContainer.transform.SetParent(transform);

            // Initialize glTFast outside try-catch (yield cannot be in try-catch)
            GLTFast.GltfImport gltfImport = null;
            try
            {
                gltfImport = new GLTFast.GltfImport();
            }
            catch (Exception ex)
            {
                loadFailed = true;
                errorMessage = $"Exception creating GltfImport: {ex.Message}";
                Debug.LogError($"[AvatarLoader] {errorMessage}");
            }

            if (loadFailed || gltfImport == null)
            {
                if (tempContainer != null) Destroy(tempContainer);
                // Clean up any existing custom avatar before fallback
                DestroyCurrentCustomAvatar();
                _animationManager?.DestroyCurrentAvatar();
                var msg = errorMessage ?? "Unknown load error";
                Debug.LogWarning($"[AvatarLoader] {msg}");
                _animationManager?.FireAvatarLoadFailed(msg);
                _animationManager?.ShowNotification(AvatarNotification.MapErrorToUserMessage(msg));
                _animationManager?.LoadDefaultAvatar();
                onComplete?.Invoke(false);
                yield break;
            }

            // Start load task
            var importSettings = new GLTFast.ImportSettings
            {
                NodeNameMethod = GLTFast.NameImportMethod.OriginalUnique
            };
            var loadTask = gltfImport.Load(uri, importSettings);
            yield return new WaitUntil(() => loadTask.IsCompleted);

            // Check load result
            if (!loadTask.IsCompletedSuccessfully || !loadTask.Result)
            {
                loadFailed = true;
                errorMessage = $"Failed to load avatar from URI: {uri}";
            }

            // Instantiate if load succeeded
            if (!loadFailed)
            {
                var instantiateTask = gltfImport.InstantiateMainSceneAsync(tempContainer.transform);
                yield return new WaitUntil(() => instantiateTask.IsCompleted);

                if (instantiateTask.IsCompletedSuccessfully)
                {
                    loadedObject = tempContainer;
                }
                else
                {
                    loadFailed = true;
                    errorMessage = "Failed to instantiate avatar model";
                }
            }

            if (loadFailed || loadedObject == null)
            {
                if (tempContainer != null) Destroy(tempContainer);
                gltfImport?.Dispose();
                // Clean up any existing custom avatar before fallback
                DestroyCurrentCustomAvatar();
                _animationManager?.DestroyCurrentAvatar();
                var msg2 = errorMessage ?? "Unknown load error";
                Debug.LogWarning($"[AvatarLoader] {msg2}");
                _animationManager?.FireAvatarLoadFailed(msg2);
                _animationManager?.ShowNotification(AvatarNotification.MapErrorToUserMessage(msg2));
                _animationManager?.LoadDefaultAvatar();
                onComplete?.Invoke(false);
                yield break;
            }

            // Dispose glTFast import resources (textures, buffers) now that instantiation is complete
            gltfImport?.Dispose();

            ProcessLoadedModel(loadedObject, uri, onComplete);
        }

        /// <summary>
        /// Processes a loaded model: validates skeleton, configures Humanoid retargeting,
        /// and applies as the current avatar. Public for testing without glTFast.
        /// </summary>
        /// <param name="model">The loaded model GameObject.</param>
        /// <param name="uri">The source URI for event reporting.</param>
        /// <param name="onComplete">Optional completion callback.</param>
        public void ProcessLoadedModel(GameObject model, string uri, Action<bool> onComplete = null)
        {
            // Validate skeleton
            var validationResult = SkeletonValidator.Validate(model.transform);
            if (!validationResult.IsValid)
            {
                Debug.LogWarning($"[AvatarLoader] Skeleton validation failed: {validationResult.Message}");
                Destroy(model);
                // Clean up any existing custom avatar before fallback
                DestroyCurrentCustomAvatar();
                _animationManager?.DestroyCurrentAvatar();
                _animationManager?.FireAvatarLoadFailed(validationResult.Message);
                _animationManager?.ShowNotification(AvatarNotification.MapErrorToUserMessage(validationResult.Message));
                _animationManager?.LoadDefaultAvatar();
                onComplete?.Invoke(false);
                return;
            }

            // Apply the custom avatar
            ApplyCustomAvatar(model, uri, onComplete);
        }

        /// <summary>
        /// Applies a validated model as the current avatar.
        /// Destroys any previous custom/default avatar, configures Animator.
        /// </summary>
        private void ApplyCustomAvatar(GameObject model, string uri, Action<bool> onComplete)
        {
            // Destroy existing custom avatar
            DestroyCurrentCustomAvatar();

            // Tell animation manager to clear current avatar (default or previous)
            _animationManager?.DestroyCurrentAvatar();

            // Parent model to entity
            model.transform.SetParent(transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            // Configure Animator on the model
            var modelAnimator = model.GetComponent<Animator>();
            if (modelAnimator == null)
            {
                modelAnimator = model.AddComponent<Animator>();
            }

            // Apply the shared controller from Resources
            var controller = Resources.Load<RuntimeAnimatorController>("AvatarAnimatorController");
            if (controller != null)
            {
                modelAnimator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning("[AvatarLoader] AvatarAnimatorController not found in Resources");
            }

            // Attempt to build Humanoid Avatar for retargeting
            TryConfigureHumanoidAvatar(model, modelAnimator);

            // Transfer Animator to animation manager (same pattern as LoadDefaultAvatar)
            _animationManager?.SetAnimator(modelAnimator);

            // Store as current custom avatar
            _currentCustomAvatar = model;
            _currentAvatarUri = uri;

            _animationManager?.FireAvatarLoaded(uri);
            Debug.Log($"[AvatarLoader] Custom avatar loaded: {uri}");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Attempts to configure a Humanoid Avatar for Mecanim retargeting.
        /// If the model already has a Humanoid Avatar, uses it directly.
        /// Otherwise, attempts to build one from the skeleton.
        /// Falls back to Generic (no retargeting) if building fails.
        /// </summary>
        private void TryConfigureHumanoidAvatar(GameObject model, Animator animator)
        {
            // Check if model already has a configured Humanoid Avatar
            if (animator.avatar != null && animator.avatar.isHuman)
            {
                Debug.Log("[AvatarLoader] Model has existing Humanoid Avatar — using directly");
                return;
            }

            // Attempt to build Humanoid Avatar from bone hierarchy
            try
            {
                var humanBones = BuildHumanBoneArray(model.transform);
                if (humanBones == null || humanBones.Length == 0)
                {
                    Debug.LogWarning("[AvatarLoader] Could not map bones for Humanoid Avatar — using Generic animation");
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
                    Debug.Log("[AvatarLoader] Built Humanoid Avatar for retargeting");
                }
                else
                {
                    Debug.LogWarning("[AvatarLoader] AvatarBuilder produced invalid result — using Generic animation");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AvatarLoader] Failed to build Humanoid Avatar: {ex.Message} — using Generic animation");
            }
        }

        /// <summary>
        /// Maps validated bone names to Unity Humanoid bone names for HumanDescription.
        /// </summary>
        private HumanBone[] BuildHumanBoneArray(Transform root)
        {
            // Required bones mapped to Unity HumanBodyBones human names
            // Keys = Transform names in the model hierarchy,
            // Values = Mecanim humanoid bone names (must match Unity's exact strings with spaces).
            var boneMapping = new Dictionary<string, string>
            {
                { "Hips", "Hips" },
                { "Spine", "Spine" },
                { "Chest", "Chest" },
                { "Head", "Head" },
                { "LeftUpperArm", "Left Upper Arm" },
                { "LeftLowerArm", "Left Lower Arm" },
                { "LeftHand", "Left Hand" },
                { "RightUpperArm", "Right Upper Arm" },
                { "RightLowerArm", "Right Lower Arm" },
                { "RightHand", "Right Hand" },
                { "LeftUpperLeg", "Left Upper Leg" },
                { "LeftLowerLeg", "Left Lower Leg" },
                { "LeftFoot", "Left Foot" },
                { "RightUpperLeg", "Right Upper Leg" },
                { "RightLowerLeg", "Right Lower Leg" },
                { "RightFoot", "Right Foot" }
            };

            // Also include optional bones if present
            var optionalMapping = new Dictionary<string, string>
            {
                { "Neck", "Neck" },
                { "UpperChest", "Upper Chest" },
                { "LeftShoulder", "Left Shoulder" },
                { "RightShoulder", "Right Shoulder" },
                { "LeftToes", "Left Toes" },
                { "RightToes", "Right Toes" }
            };

            // Collect all transform names in hierarchy
            var nameToTransform = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            CollectTransformNames(root, nameToTransform);

            var humanBones = new List<HumanBone>();

            // Map required bones (using SkeletonValidator aliases for VRM/Mixamo support)
            foreach (var kvp in boneMapping)
            {
                var actualName = FindBoneByAlias(kvp.Key, nameToTransform);
                if (actualName != null)
                {
                    humanBones.Add(new HumanBone
                    {
                        humanName = kvp.Value,
                        boneName = actualName,
                        limit = new HumanLimit { useDefaultValues = true }
                    });
                }
            }

            // Map optional bones (using aliases)
            foreach (var kvp in optionalMapping)
            {
                var actualName = FindBoneByAlias(kvp.Key, nameToTransform);
                if (actualName != null)
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

        /// <summary>
        /// Builds the SkeletonBone array from the model's transform hierarchy.
        /// </summary>
        private SkeletonBone[] BuildSkeletonBoneArray(Transform root)
        {
            var skeletonBones = new List<SkeletonBone>();
            CollectSkeletonBones(root, skeletonBones);
            return skeletonBones.ToArray();
        }

        private void CollectSkeletonBones(Transform current, List<SkeletonBone> bones)
        {
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

        /// <summary>
        /// Finds a bone's actual name in the hierarchy using SkeletonValidator's alias table.
        /// Returns the actual transform name if found, or null if not present.
        /// </summary>
        private string FindBoneByAlias(string requiredBone, Dictionary<string, string> nameToTransform)
        {
            // Direct match first
            if (nameToTransform.TryGetValue(requiredBone, out var actualName))
            {
                return actualName;
            }

            // Check aliases from SkeletonValidator
            if (SkeletonValidator.BoneAliases.TryGetValue(requiredBone, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    if (nameToTransform.TryGetValue(alias, out actualName))
                    {
                        return actualName;
                    }
                }
            }

            return null;
        }

        private void CollectTransformNames(Transform current, Dictionary<string, string> names)
        {
            // Store with case-insensitive key, actual name as value
            if (!names.ContainsKey(current.name))
            {
                names[current.name] = current.name;
            }

            foreach (Transform child in current)
            {
                CollectTransformNames(child, names);
            }
        }

        /// <summary>
        /// Destroys the current custom avatar if one exists.
        /// </summary>
        public void DestroyCurrentCustomAvatar()
        {
            if (_currentCustomAvatar != null)
            {
                Destroy(_currentCustomAvatar);
                _currentCustomAvatar = null;
                _currentAvatarUri = null;
            }
        }

        /// <summary>
        /// Cleans up the loader — destroys any custom avatar.
        /// </summary>
        public void Cleanup()
        {
            DestroyCurrentCustomAvatar();
        }
    }
}
