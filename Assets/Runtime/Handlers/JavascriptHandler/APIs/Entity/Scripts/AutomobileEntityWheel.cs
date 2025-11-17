// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for an automobile entity wheel.
    /// </summary>
    public class AutomobileEntityWheel
    {
        /// <summary>
        /// Submesh corresponding to the wheel.
        /// </summary>
        public string wheelSubMesh;

        /// <summary>
        /// Radius of the wheel.
        /// </summary>
        public float wheelRadius;

        /// <summary>
        /// Create an automobile entity wheel.
        /// </summary>
        /// <param name="wheelSubMesh">Submesh corresponding to the wheel.</param>
        /// <param name="wheelRadius">Radius of the wheel.</param>
        public AutomobileEntityWheel(string wheelSubMesh, float wheelRadius)
        {
            this.wheelSubMesh = wheelSubMesh;
            this.wheelRadius = wheelRadius;
        }
    }
}