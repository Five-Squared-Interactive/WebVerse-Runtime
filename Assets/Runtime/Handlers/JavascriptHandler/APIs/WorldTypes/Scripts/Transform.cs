// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a transform.
    /// </summary>
    public class Transform
    {
        /// <summary>
        /// Position.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Rotation.
        /// </summary>
        public Quaternion rotation;
        
        /// <summary>
        /// Scale.
        /// </summary>
        public Vector3 scale;

        /// <summary>
        /// The forward vector for the transform.
        /// </summary>
        public Vector3 forward
        {
            get
            {
                UnityEngine.GameObject testGO = new UnityEngine.GameObject("test");
                testGO.transform.position = new UnityEngine.Vector3(position.x, position.y, position.z);
                testGO.transform.rotation = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
                testGO.transform.localScale = new UnityEngine.Vector3(scale.x, scale.y, scale.z);
                UnityEngine.Vector3 fwd = testGO.transform.forward;
                Vector3 rtn = new Vector3(fwd.x, fwd.y, fwd.z);
                UnityEngine.Object.Destroy(testGO);
                return rtn;
            }
        }

        /// <summary>
        /// The right vector for the transform.
        /// </summary>
        public Vector3 right
        {
            get
            {
                UnityEngine.GameObject testGO = new UnityEngine.GameObject("test");
                testGO.transform.position = new UnityEngine.Vector3(position.x, position.y, position.z);
                testGO.transform.rotation = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
                testGO.transform.localScale = new UnityEngine.Vector3(scale.x, scale.y, scale.z);
                UnityEngine.Vector3 right = testGO.transform.right;
                Vector3 rtn = new Vector3(right.x, right.y, right.z);
                UnityEngine.Object.Destroy(testGO);
                return rtn;
            }
        }
    }
}