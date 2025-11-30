// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;

namespace FiveSQD.WebVerse.X3D.Abstraction
{
    /// <summary>
    /// Light types supported by X3D.
    /// </summary>
    public enum X3DLightType
    {
        Directional,
        Point,
        Spot
    }

    /// <summary>
    /// Result of an entity creation operation.
    /// </summary>
    public class X3DEntityHandle
    {
        /// <summary>
        /// Unique identifier for the created entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Optional tag/name for the entity (from DEF attribute).
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Whether the entity was successfully created.
        /// </summary>
        public bool Success { get; set; }
    }

    /// <summary>
    /// Configuration for fog settings.
    /// </summary>
    public struct X3DFogSettings
    {
        public bool Enabled;
        public Color Color;
        public float VisibilityRange;
        public string FogType; // "LINEAR" or "EXPONENTIAL"
    }

    /// <summary>
    /// Configuration for skybox/background.
    /// X3D Background uses arrays of colors with angles to create gradients.
    /// skyColor[0] is at zenith (straight up), additional colors are at skyAngle[i] radians from zenith.
    /// groundColor[0] is at nadir (straight down), additional colors are at groundAngle[i] radians from nadir.
    /// </summary>
    public struct X3DBackgroundSettings
    {
        /// <summary>Sky colors from zenith down. First color is at zenith (0 radians).</summary>
        public Color[] SkyColors;
        /// <summary>Angles in radians from zenith for skyColors[1..n]. Length is skyColors.Length - 1.</summary>
        public float[] SkyAngles;
        /// <summary>Ground colors from nadir up. First color is at nadir (0 radians).</summary>
        public Color[] GroundColors;
        /// <summary>Angles in radians from nadir for groundColors[1..n]. Length is groundColors.Length - 1.</summary>
        public float[] GroundAngles;
        public string[] FrontUrl;
        public string[] BackUrl;
        public string[] LeftUrl;
        public string[] RightUrl;
        public string[] TopUrl;
        public string[] BottomUrl;

        /// <summary>Get the primary sky color (at zenith), or black if none.</summary>
        public Color PrimarySkyColor => (SkyColors != null && SkyColors.Length > 0) ? SkyColors[0] : Color.black;
        /// <summary>Get the primary ground color (at nadir), or black if none.</summary>
        public Color PrimaryGroundColor => (GroundColors != null && GroundColors.Length > 0) ? GroundColors[0] : Color.black;
        /// <summary>Get the horizon sky color (last sky color), or primary if only one.</summary>
        public Color HorizonSkyColor => (SkyColors != null && SkyColors.Length > 0) ? SkyColors[SkyColors.Length - 1] : Color.black;
        /// <summary>Get the horizon ground color (last ground color), or primary if only one.</summary>
        public Color HorizonGroundColor => (GroundColors != null && GroundColors.Length > 0) ? GroundColors[GroundColors.Length - 1] : Color.black;
    }

    /// <summary>
    /// Abstract interface for any world engine that X3D can target.
    /// This interface decouples the X3D library from specific world engine implementations.
    /// </summary>
    public interface IX3DWorldAdapter
    {
        #region Entity Creation

        /// <summary>
        /// Create a container/group entity (Transform, Group nodes).
        /// </summary>
        /// <param name="parent">Parent entity handle, or null for root.</param>
        /// <param name="position">World or local position.</param>
        /// <param name="rotation">Rotation as quaternion.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="tag">Optional tag/name (from DEF attribute).</param>
        /// <param name="onCreated">Callback when entity is ready.</param>
        /// <returns>Handle to the created entity.</returns>
        X3DEntityHandle CreateContainerEntity(
            X3DEntityHandle parent,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null);

        /// <summary>
        /// Create a mesh entity from geometry data.
        /// </summary>
        /// <param name="parent">Parent entity handle, or null for root.</param>
        /// <param name="mesh">Unity Mesh containing geometry.</param>
        /// <param name="material">Material to apply.</param>
        /// <param name="position">World or local position.</param>
        /// <param name="rotation">Rotation as quaternion.</param>
        /// <param name="scale">Scale factor.</param>
        /// <param name="tag">Optional tag/name (from DEF attribute).</param>
        /// <param name="onCreated">Callback when entity is ready.</param>
        /// <returns>Handle to the created entity.</returns>
        X3DEntityHandle CreateMeshEntity(
            X3DEntityHandle parent,
            Mesh mesh,
            Material material,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null);

        /// <summary>
        /// Create a light entity.
        /// </summary>
        /// <param name="parent">Parent entity handle, or null for root.</param>
        /// <param name="lightType">Type of light (Directional, Point, Spot).</param>
        /// <param name="color">Light color.</param>
        /// <param name="intensity">Light intensity.</param>
        /// <param name="position">Position for point/spot lights.</param>
        /// <param name="rotation">Direction for directional/spot lights.</param>
        /// <param name="range">Range for point/spot lights.</param>
        /// <param name="spotAngle">Cone angle for spot lights.</param>
        /// <param name="tag">Optional tag/name (from DEF attribute).</param>
        /// <param name="onCreated">Callback when entity is ready.</param>
        /// <returns>Handle to the created entity.</returns>
        X3DEntityHandle CreateLightEntity(
            X3DEntityHandle parent,
            X3DLightType lightType,
            Color color,
            float intensity,
            Vector3 position,
            Quaternion rotation,
            float range = 10f,
            float spotAngle = 45f,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null);

        /// <summary>
        /// Create a terrain entity from height data.
        /// </summary>
        /// <param name="parent">Parent entity handle, or null for root.</param>
        /// <param name="heights">2D array of normalized height values.</param>
        /// <param name="width">Terrain width in world units.</param>
        /// <param name="length">Terrain length in world units.</param>
        /// <param name="height">Maximum terrain height.</param>
        /// <param name="position">Terrain position.</param>
        /// <param name="tag">Optional tag/name (from DEF attribute).</param>
        /// <param name="onCreated">Callback when entity is ready.</param>
        /// <returns>Handle to the created entity.</returns>
        X3DEntityHandle CreateTerrainEntity(
            X3DEntityHandle parent,
            float[,] heights,
            float width,
            float length,
            float height,
            Vector3 position,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null);

        /// <summary>
        /// Create an audio/sound entity.
        /// </summary>
        /// <param name="parent">Parent entity handle, or null for root.</param>
        /// <param name="position">Position for spatial audio.</param>
        /// <param name="direction">Direction for directional audio.</param>
        /// <param name="minDistance">Minimum distance for audio falloff.</param>
        /// <param name="maxDistance">Maximum distance for audio falloff.</param>
        /// <param name="volume">Audio volume (0-1).</param>
        /// <param name="spatialize">Whether audio is spatialized.</param>
        /// <param name="tag">Optional tag/name (from DEF attribute).</param>
        /// <param name="onCreated">Callback when entity is ready.</param>
        /// <returns>Handle to the created entity.</returns>
        X3DEntityHandle CreateAudioEntity(
            X3DEntityHandle parent,
            Vector3 position,
            Vector3 direction,
            float minDistance,
            float maxDistance,
            float volume,
            bool spatialize,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null);

        #endregion

        #region Environment

        /// <summary>
        /// Set the skybox/background.
        /// </summary>
        /// <param name="settings">Background configuration.</param>
        void SetBackground(X3DBackgroundSettings settings);

        /// <summary>
        /// Set fog settings.
        /// </summary>
        /// <param name="settings">Fog configuration.</param>
        void SetFog(X3DFogSettings settings);

        /// <summary>
        /// Set ambient lighting.
        /// </summary>
        /// <param name="color">Ambient light color.</param>
        /// <param name="intensity">Ambient light intensity.</param>
        void SetAmbientLight(Color color, float intensity);

        #endregion

        #region Camera/Viewpoint

        /// <summary>
        /// Set the camera/viewpoint position and orientation.
        /// </summary>
        /// <param name="position">Camera position.</param>
        /// <param name="rotation">Camera rotation.</param>
        /// <param name="fieldOfView">Field of view in radians.</param>
        /// <param name="description">Optional viewpoint description.</param>
        void SetViewpoint(Vector3 position, Quaternion rotation, float fieldOfView, string description = null);

        #endregion

        #region Entity Manipulation

        /// <summary>
        /// Set the parent of an entity.
        /// </summary>
        /// <param name="entity">Entity to reparent.</param>
        /// <param name="newParent">New parent entity, or null for root.</param>
        void SetEntityParent(X3DEntityHandle entity, X3DEntityHandle newParent);

        /// <summary>
        /// Set the transform of an entity.
        /// </summary>
        /// <param name="entity">Entity to transform.</param>
        /// <param name="position">New position.</param>
        /// <param name="rotation">New rotation.</param>
        /// <param name="scale">New scale.</param>
        void SetEntityTransform(X3DEntityHandle entity, Vector3 position, Quaternion rotation, Vector3 scale);

        /// <summary>
        /// Set the visibility of an entity.
        /// </summary>
        /// <param name="entity">Entity to modify.</param>
        /// <param name="visible">Whether entity should be visible.</param>
        void SetEntityVisibility(X3DEntityHandle entity, bool visible);

        /// <summary>
        /// Delete an entity and its children.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        void DeleteEntity(X3DEntityHandle entity);

        /// <summary>
        /// Find an entity by its tag/DEF name.
        /// </summary>
        /// <param name="tag">Tag to search for.</param>
        /// <returns>Entity handle if found, null otherwise.</returns>
        X3DEntityHandle FindEntityByTag(string tag);

        #endregion

        #region Resource Loading

        /// <summary>
        /// Load a texture asynchronously.
        /// </summary>
        /// <param name="url">URL or path to texture.</param>
        /// <param name="onLoaded">Callback with loaded texture (null if failed).</param>
        void LoadTextureAsync(string url, Action<Texture2D> onLoaded);

        /// <summary>
        /// Load an audio clip asynchronously.
        /// </summary>
        /// <param name="url">URL or path to audio file.</param>
        /// <param name="onLoaded">Callback with loaded audio clip (null if failed).</param>
        void LoadAudioClipAsync(string url, Action<AudioClip> onLoaded);

        /// <summary>
        /// Load a 3D model/mesh asynchronously (for Inline nodes).
        /// </summary>
        /// <param name="url">URL or path to model file.</param>
        /// <param name="onLoaded">Callback with loaded GameObject (null if failed).</param>
        void LoadModelAsync(string url, Action<GameObject> onLoaded);

        #endregion

        #region Material Creation

        /// <summary>
        /// Create a material from X3D material properties.
        /// </summary>
        /// <param name="diffuseColor">Diffuse/albedo color.</param>
        /// <param name="emissiveColor">Emissive color.</param>
        /// <param name="specularColor">Specular color.</param>
        /// <param name="ambientIntensity">Ambient intensity.</param>
        /// <param name="shininess">Shininess/smoothness.</param>
        /// <param name="transparency">Transparency (0 = opaque, 1 = fully transparent).</param>
        /// <returns>Created Unity Material.</returns>
        Material CreateMaterial(
            Color diffuseColor,
            Color emissiveColor,
            Color specularColor,
            float ambientIntensity,
            float shininess,
            float transparency);

        /// <summary>
        /// Apply a texture to a material.
        /// </summary>
        /// <param name="material">Material to modify.</param>
        /// <param name="texture">Texture to apply.</param>
        /// <param name="repeatS">Whether to repeat texture in S direction.</param>
        /// <param name="repeatT">Whether to repeat texture in T direction.</param>
        void ApplyTextureToMaterial(Material material, Texture2D texture, bool repeatS, bool repeatT);

        #endregion
    }
}
