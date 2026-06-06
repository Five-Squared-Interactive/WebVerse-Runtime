// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Static utility that validates a Transform hierarchy against Unity Humanoid bone requirements.
    /// Recognizes Unity Humanoid, VRM 1.0, Mixamo, and common alternative bone naming conventions.
    /// </summary>
    public static class SkeletonValidator
    {
        /// <summary>
        /// The 15 bones required by Unity Mecanim for Humanoid Avatar mapping.
        /// </summary>
        private static readonly string[] RequiredBones = new[]
        {
            "Hips", "Spine", "Chest", "Head",
            "LeftUpperArm", "LeftLowerArm", "LeftHand",
            "RightUpperArm", "RightLowerArm", "RightHand",
            "LeftUpperLeg", "LeftLowerLeg", "LeftFoot",
            "RightUpperLeg", "RightLowerLeg", "RightFoot"
        };

        /// <summary>
        /// Maps each required bone to its known alternative names (VRM, Mixamo, etc.).
        /// All alias lookups are case-insensitive.
        /// Internal for use by AvatarLoader's Humanoid bone mapping.
        /// </summary>
        internal static readonly Dictionary<string, string[]> BoneAliases = new Dictionary<string, string[]>(
            StringComparer.OrdinalIgnoreCase)
        {
            { "Hips", new[] { "J_Bip_C_Hips", "mixamorig:Hips", "Bip01_Pelvis", "pelvis" } },
            { "Spine", new[] { "J_Bip_C_Spine", "mixamorig:Spine", "Bip01_Spine", "spine_01" } },
            { "Chest", new[] { "J_Bip_C_Chest", "mixamorig:Spine1", "Bip01_Spine1", "spine_02" } },
            { "Head", new[] { "J_Bip_C_Head", "mixamorig:Head", "Bip01_Head", "head" } },
            { "LeftUpperArm", new[] { "J_Bip_L_UpperArm", "mixamorig:LeftArm", "Bip01_L_UpperArm", "upperarm_l" } },
            { "LeftLowerArm", new[] { "J_Bip_L_LowerArm", "mixamorig:LeftForeArm", "Bip01_L_Forearm", "lowerarm_l" } },
            { "LeftHand", new[] { "J_Bip_L_Hand", "mixamorig:LeftHand", "Bip01_L_Hand", "hand_l" } },
            { "RightUpperArm", new[] { "J_Bip_R_UpperArm", "mixamorig:RightArm", "Bip01_R_UpperArm", "upperarm_r" } },
            { "RightLowerArm", new[] { "J_Bip_R_LowerArm", "mixamorig:RightForeArm", "Bip01_R_Forearm", "lowerarm_r" } },
            { "RightHand", new[] { "J_Bip_R_Hand", "mixamorig:RightHand", "Bip01_R_Hand", "hand_r" } },
            { "LeftUpperLeg", new[] { "J_Bip_L_UpperLeg", "mixamorig:LeftUpLeg", "Bip01_L_Thigh", "thigh_l" } },
            { "LeftLowerLeg", new[] { "J_Bip_L_LowerLeg", "mixamorig:LeftLeg", "Bip01_L_Calf", "calf_l" } },
            { "LeftFoot", new[] { "J_Bip_L_Foot", "mixamorig:LeftFoot", "Bip01_L_Foot", "foot_l" } },
            { "RightUpperLeg", new[] { "J_Bip_R_UpperLeg", "mixamorig:RightUpLeg", "Bip01_R_Thigh", "thigh_r" } },
            { "RightLowerLeg", new[] { "J_Bip_R_LowerLeg", "mixamorig:RightLeg", "Bip01_R_Calf", "calf_r" } },
            { "RightFoot", new[] { "J_Bip_R_Foot", "mixamorig:RightFoot", "Bip01_R_Foot", "foot_r" } },
        };

        /// <summary>
        /// Validates that the given Transform hierarchy contains all required humanoid bones.
        /// Recognizes Unity Humanoid, VRM 1.0, Mixamo, and common alternative naming conventions.
        /// Case-insensitive matching.
        /// </summary>
        /// <param name="root">Root transform of the skeleton hierarchy to validate.</param>
        /// <returns>A SkeletonValidationResult indicating whether the skeleton is valid.</returns>
        public static SkeletonValidationResult Validate(Transform root)
        {
            try
            {
                if (root == null)
                {
                    Debug.LogWarning("[SkeletonValidator] Root transform is null");
                    return SkeletonValidationResult.Invalid(
                        "Root transform is null",
                        new List<string>());
                }

                // Collect all transform names in hierarchy (case-insensitive)
                var boneNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                CollectBoneNames(root, boneNames);

                // Check each required bone
                var missingBones = new List<string>();
                foreach (var requiredBone in RequiredBones)
                {
                    if (!IsBonePresent(requiredBone, boneNames))
                    {
                        missingBones.Add(requiredBone);
                        Debug.LogWarning($"[SkeletonValidator] Missing required bone: {requiredBone}");
                    }
                }

                if (missingBones.Count == 0)
                {
                    return SkeletonValidationResult.Valid();
                }

                string message = $"Skeleton is missing {missingBones.Count} required bone(s): {string.Join(", ", missingBones)}";
                return SkeletonValidationResult.Invalid(message, missingBones);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SkeletonValidator] Unexpected error during validation: {ex.Message}");
                return SkeletonValidationResult.Invalid(
                    $"Validation error: {ex.Message}",
                    new List<string>());
            }
        }

        /// <summary>
        /// Recursively collects all Transform names in the hierarchy.
        /// </summary>
        private static void CollectBoneNames(Transform current, HashSet<string> names)
        {
            names.Add(current.name);
            foreach (Transform child in current)
            {
                CollectBoneNames(child, names);
            }
        }

        /// <summary>
        /// Checks if a required bone is present, considering all known aliases.
        /// </summary>
        private static bool IsBonePresent(string requiredBone, HashSet<string> boneNames)
        {
            // Direct match (case-insensitive via HashSet comparer)
            if (boneNames.Contains(requiredBone))
            {
                return true;
            }

            // Check aliases
            if (BoneAliases.TryGetValue(requiredBone, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    if (boneNames.Contains(alias))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
