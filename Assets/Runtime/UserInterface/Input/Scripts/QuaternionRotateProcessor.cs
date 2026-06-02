// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.InputSystem;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Input System processor that applies an Euler rotation offset to quaternion values.
    /// Used to correct OpenXR controller pose orientation before it reaches TrackedPoseDriver
    /// and XRI interactors, ensuring all downstream systems see the corrected pose.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class QuaternionRotateProcessor : InputProcessor<Quaternion>
    {
        public float x;
        public float y;
        public float z;

        public override Quaternion Process(Quaternion value, InputControl control)
        {
            if (x == 0f && y == 0f && z == 0f)
                return value;

            return value * Quaternion.Euler(x, y, z);
        }

#if UNITY_EDITOR
        static QuaternionRotateProcessor()
        {
            Initialize();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            InputSystem.RegisterProcessor<QuaternionRotateProcessor>();
        }
    }
}
