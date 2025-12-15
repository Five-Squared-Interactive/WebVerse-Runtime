// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour
{
    /// <summary>
    /// Constants for CustomData keys used to share data between StraightFour handlers.
    /// These keys are used with OMIImportContext.CustomData dictionary.
    /// </summary>
    public static class StraightFourCustomDataKeys
    {
        /// <summary>
        /// Key prefix for all StraightFour data.
        /// </summary>
        public const string Prefix = "SF_";

        /// <summary>
        /// Dictionary&lt;int, BaseEntity&gt; mapping node indices to StraightFour entities.
        /// </summary>
        public const string NodeToEntity = Prefix + "NodeToEntity";

        /// <summary>
        /// List&lt;OMIPhysicsShape&gt; from document-level OMI_physics_shape.
        /// </summary>
        public const string PhysicsShapes = Prefix + "PhysicsShapes";

        /// <summary>
        /// List&lt;OMIPhysicsMaterial&gt; from document-level OMI_physics_body.
        /// </summary>
        public const string PhysicsMaterials = Prefix + "PhysicsMaterials";

        /// <summary>
        /// List&lt;OMICollisionFilter&gt; from document-level OMI_physics_body.
        /// </summary>
        public const string CollisionFilters = Prefix + "CollisionFilters";

        /// <summary>
        /// List&lt;OMIPhysicsJointSettings&gt; from document-level OMI_physics_joint.
        /// </summary>
        public const string JointSettings = Prefix + "JointSettings";

        /// <summary>
        /// OMIPhysicsGravityRoot from document-level OMI_physics_gravity.
        /// </summary>
        public const string WorldGravity = Prefix + "WorldGravity";

        /// <summary>
        /// List&lt;OMIEnvironmentSkySkyData&gt; from document-level OMI_environment_sky.
        /// </summary>
        public const string Skies = Prefix + "Skies";

        /// <summary>
        /// List&lt;KHRAudioData&gt; audio data resources.
        /// </summary>
        public const string AudioData = Prefix + "AudioData";

        /// <summary>
        /// List&lt;KHRAudioSource&gt; audio source definitions.
        /// </summary>
        public const string AudioSources = Prefix + "AudioSources";

        /// <summary>
        /// List&lt;AudioClip&gt; loaded audio clips.
        /// </summary>
        public const string LoadedAudioClips = Prefix + "LoadedAudioClips";

        /// <summary>
        /// List&lt;OMIVehicleWheelSettings&gt; from document-level OMI_vehicle_wheel.
        /// </summary>
        public const string WheelSettings = Prefix + "WheelSettings";

        /// <summary>
        /// WebVerseRuntime reference for handlers that need it.
        /// </summary>
        public const string Runtime = Prefix + "Runtime";

        /// <summary>
        /// SpawnPointRegistry reference for spawn point handler.
        /// </summary>
        public const string SpawnPointRegistry = Prefix + "SpawnPointRegistry";
    }
}
