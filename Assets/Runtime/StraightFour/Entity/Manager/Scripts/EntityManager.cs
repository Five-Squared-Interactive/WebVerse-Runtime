// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using FiveSQD.StraightFour.Utilities;
using UnityEngine.Audio;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// The Entity Manager class manages all of the entities in a world and
    /// serves as the top-level entity interface to other components.
    /// </summary>
    public class EntityManager : BaseManager
    {
        /// <summary>
        /// Enumeration for an automobile entity type.
        /// </summary>
        [Tooltip("Enumeration for an automobile entity type.")]
        public enum AutomobileEntityType { Default = 0 };

        /// <summary>
        /// Map for automobile entity types to their NWH State Settings object.
        /// </summary>
        [Tooltip("Map for automobile entity types to their NWH State Settings object.")]
        public Dictionary<AutomobileEntityType, NWH.VehiclePhysics2.StateSettings> automobileEntityTypeMap;

        /// <summary>
        /// Audio mixer for automobile.
        /// </summary>
        [Tooltip("Audio mixer for automobile.")]
        public AudioMixer automobileAudioMixer;

        /// <summary>
        /// Prefab for a character controller.
        /// </summary>
        [Tooltip("Prefab for a character controller.")]
        public GameObject characterControllerPrefab;

        /// <summary>
        /// Prefab for a character controller label.
        /// </summary>
        [Tooltip("Prefab for a character controller label.")]
        public GameObject characterControllerLabelPrefab;

        /// <summary>
        /// Prefab for an input entity.
        /// </summary>
        [Tooltip("Prefab for an input entity.")]
        public GameObject inputEntityPrefab;

        /// <summary>
        /// Prefab for a WebView.
        /// </summary>
        [Tooltip("Prefab for a WebView.")]
        public GameObject webViewPrefab;

        /// <summary>
        /// Prefab for a Canvas WebView.
        /// </summary>
        [Tooltip("Prefab for a Canvas WebView.")]
        public GameObject canvasWebViewPrefab;

        /// <summary>
        /// Prefab for a voxel.
        /// </summary>
        [Tooltip("Prefab for a voxel.")]
        public GameObject voxelPrefab;

        /// <summary>
        /// Prefab for a water body.
        /// </summary>
        [Tooltip("Prefab for a water body.")]
        public GameObject waterBodyPrefab;

        /// <summary>
        /// Prefab for a water blocker.
        /// </summary>
        [Tooltip("Prefab for a water blocker.")]
        public GameObject waterBlockerPrefab;

        /// <summary>
        /// Prefab for an airplane entity.
        /// </summary>
        [Tooltip("Prefab for an airplane entity.")]
        public GameObject airplaneEntityPrefab;

        /// <summary>
        /// Dictionary of loaded entities.
        /// </summary>
        private Dictionary<Guid, BaseEntity> entities = new Dictionary<Guid, BaseEntity>();
        
        /// <summary>
        /// Get a new entity ID.
        /// </summary>
        private Guid GetEntityID()
        {
            return Guid.NewGuid();
        }

        // Initialize the Entity Manager.
        public override void Initialize()
        {
            
        }
        
        /// <summary>
        /// Load a container entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the container entity.</param>
        /// <param name="position">Position to apply to the container entity.</param>
        /// <param name="rotation">Rotation to apply to the container entity.</param>
        /// <param name="scale">Scale/size to apply to the container entity.</param>
        /// <param name="id">ID to apply to the container entity.</param>
        /// <param name="isSize">Whether or not the scale value is for a size.</param>
        /// <param name="tag">Tag to give the container entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new container entity.</returns>
        public Guid LoadContainerEntity(BaseEntity parentEntity,
            Vector3 position, Quaternion rotation, Vector3 scale, Guid? id = null,
            string tag = null, bool isSize = false, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadEmptyEntity(entityID, parentEntity,
                position, rotation, scale, isSize, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load a character entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the character entity.</param>
        /// <param name="meshPrefab">Prefab to load mesh character entity from.</param>
        /// <param name="meshOffset">Offset for the mesh character entity object.</param>
        /// <param name="meshRotation">Rotation for the mesh character entity object.</param>
        /// <param name="avatarLabelOffset">Offset for the mesh character entity's label.</param>
        /// <param name="position">Position to apply to the character entity.</param>
        /// <param name="rotation">Rotation to apply to the character entity.</param>
        /// <param name="scale">Scale/size to apply to the character entity.</param>
        /// <param name="id">ID to apply to the character entity.</param>
        /// <param name="tag">Tag to apply to the character entity.</param>
        /// <param name="isSize">Whether or not the scale value is for a size.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new character entity.</returns>
        public Guid LoadCharacterEntity(BaseEntity parentEntity,
            GameObject meshPrefab, Vector3 meshOffset, Quaternion meshRotation,
            Vector3 avatarLabelOffset, Vector3 position, Quaternion rotation,
            Vector3 scale, Guid? id = null, string tag = null, bool isSize = false,
            Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            if (meshPrefab == null)
            {
                StartCoroutine(LoadDefaultCharacterEntity(entityID, tag,
                    parentEntity, position, rotation, scale, isSize, onLoaded));
            }
            else
            {
                StartCoroutine(LoadMeshCharacterEntity(entityID, tag, parentEntity,
                    meshPrefab, meshOffset, meshRotation, avatarLabelOffset, position, rotation, scale, isSize, onLoaded));
            }
            return entityID;
        }

        /// <summary>
        /// Load a light entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the light entity.</param>
        /// <param name="position">Position to apply to the light entity.</param>
        /// <param name="rotation">Rotation to apply to the light entity.</param>
        /// <param name="id">ID to apply to the light entity.</param>
        /// <param name="tag">Tag to apply to the light entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new light entity.</returns>
        public Guid LoadLightEntity(BaseEntity parentEntity,
            Vector3 position, Quaternion rotation, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadLightEntity(entityID, parentEntity,
                position, rotation, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load a mesh entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the mesh entity.</param>
        /// <param name="meshPrefab">Prefab to load mesh entity from.</param>
        /// <param name="position">Position to apply to the mesh entity.</param>
        /// <param name="rotation">Rotation to apply to the mesh entity.</param>
        /// <param name="id">ID to apply to the mesh entity.</param>
        /// <param name="tag">Tag to apply to the mesh entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new mesh entity.</returns>
        public Guid LoadMeshEntity(BaseEntity parentEntity, GameObject meshPrefab,
            Vector3 position, Quaternion rotation, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadMeshEntity(meshPrefab, entityID, parentEntity,
                position, rotation, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load a terrain entity.
        /// </summary>
        /// <param name="length">Length of the terrain.</param>
        /// <param name="width">Width of the terrain.</param>
        /// <param name="height">Height of the terrain.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="parentEntity">Parent entity to give the terrain entity.</param>
        /// <param name="position">Position to apply to the terrain entity.</param>
        /// <param name="rotation">Rotation to apply to the terrain entity.</param>
        /// <param name="id">ID to apply to the terrain entity.</param>
        /// <param name="tag">Tag to apply to the terrain entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new terrain entity.</returns>
        public Guid LoadTerrainEntity(float length, float width, float height,
            float[,] heights, Terrain.TerrainEntityLayer[] layers, Dictionary<int, float[,]> layerMasks,
            BaseEntity parentEntity, Vector3 position, Quaternion rotation, bool stitchTerrain, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadTerrainEntity(length, width, height, heights, layers, layerMasks,
                entityID, parentEntity, position, rotation, stitchTerrain, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load a terrain entity.
        /// </summary>
        /// <param name="length">Length of the terrain.</param>
        /// <param name="width">Width of the terrain.</param>
        /// <param name="height">Height of the terrain.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="parentEntity">Parent entity to give the terrain entity.</param>
        /// <param name="position">Position to apply to the terrain entity.</param>
        /// <param name="rotation">Rotation to apply to the terrain entity.</param>
        /// <param name="id">ID to apply to the terrain entity.</param>
        /// <param name="tag">Tag to apply to the terrain entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new terrain entity.</returns>
        public Guid LoadTerrainEntity(float length, float width, float height,
            float[][] heights, Terrain.TerrainEntityLayer[] layers, Dictionary<int, float[,]> layerMasks,
            BaseEntity parentEntity, Vector3 position, Quaternion rotation, bool stitchTerrain, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            if (heights == null || heights[0] == null)
            {
                LogSystem.LogWarning("[EntityManager->LoadTerrainEntity] Invalid heights array.");
                return Guid.Empty;
            }

            float[,] processedHeights = new float[heights.Length, heights[0].Length];
            for (int i = 0; i < heights.Length; i++)
            {
                for (int j = 0; j < heights[0].Length; j++)
                {
                    processedHeights[i, j] = heights[i][j];
                }
            }

            return LoadTerrainEntity(length, width, height, processedHeights, layers, layerMasks,
                parentEntity, position, rotation, stitchTerrain, id, tag, onLoaded);
        }

        /// <summary>
        /// Load a terrain entity.
        /// </summary>
        /// <param name="length">Length of the terrain.</param>
        /// <param name="width">Width of the terrain.</param>
        /// <param name="height">Height of the terrain.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="parentEntity">Parent entity to give the terrain entity.</param>
        /// <param name="position">Position to apply to the terrain entity.</param>
        /// <param name="rotation">Rotation to apply to the terrain entity.</param>
        /// <param name="id">ID to apply to the terrain entity.</param>
        /// <param name="tag">Tag to apply to the terrain entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new terrain entity.</returns>
        public Guid LoadHybridTerrainEntity(float length, float width, float height,
            float[,] heights, Terrain.TerrainEntityLayer[] layers, Dictionary<int, float[,]> layerMasks,
            BaseEntity parentEntity, Vector3 position, Quaternion rotation, bool stitchTerrains,
            Guid? id = null, string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadHybridTerrainEntity(length, width, height, heights, layers,
                layerMasks, entityID, parentEntity, position, rotation, stitchTerrains, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load a water body entity.
        /// </summary>
        /// <param name="shallowColor">Color for the shallow zone.</param>
        /// <param name="deepColor">Color for the deep zone.</param>
        /// <param name="specularColor">Specular color.</param>
        /// <param name="scatteringColor">Scattering color.</param>
        /// <param name="deepStart">Start of deep zone.</param>
        /// <param name="deepEnd">End of deep zone.</param>
        /// <param name="distortion">Distortion factor (range 0-128).</param>
        /// <param name="smoothness">Smoothness factor (range 0-1).</param>
        /// <param name="numWaves">Number of waves (range 1-32).</param>
        /// <param name="waveAmplitude">Wave amplitude (range 0-1).</param>
        /// <param name="waveSteepness">Wave steepness (range 0-1).</param>
        /// <param name="waveSpeed">Wave speed.</param>
        /// <param name="waveLength">Wave length.</param>
        /// <param name="waveScale">Scale of the waves.</param>
        /// <param name="intensity">Intensity factor (range 0-1).</param>
        /// <param name="parentEntity">Parent entity to give the water body entity.</param>
        /// <param name="position">Position to apply to the water body entity.</param>
        /// <param name="rotation">Rotation to apply to the water body entity.</param>
        /// <param name="id">ID to apply to the water body entity.</param>
        /// <param name="tag">Tag to apply to the water body entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new water body entity.</returns>
        public Guid LoadWaterBodyEntity(Color shallowColor, Color deepColor,
            Color specularColor, Color scatteringColor, float deepStart, float deepEnd, float distortion,
            float smoothness, float numWaves, float waveAmplitude, float waveSteepness, float waveSpeed,
            float waveLength, float scale, float intensity, BaseEntity parentEntity, Vector3 position, Quaternion rotation,
            Guid? id = null, string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadWaterBodyEntity(shallowColor, deepColor, specularColor, scatteringColor,
            deepStart, deepEnd, distortion, smoothness, numWaves, waveAmplitude, waveSteepness, waveSpeed,
            waveLength, scale, intensity, entityID, parentEntity, position, rotation, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load a water blocker entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the water blocker entity.</param>
        /// <param name="position">Position to apply to the water blocker entity.</param>
        /// <param name="rotation">Rotation to apply to the water blocker entity.</param>
        /// <param name="id">ID to apply to the water blocker entity.</param>
        /// <param name="tag">Tag to apply to the water blocker entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new water blocker entity.</returns>
        public Guid LoadWaterBlockerEntity(BaseEntity parentEntity, Vector3 position, Quaternion rotation,
            Guid? id = null, string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadWaterBlockerEntity(entityID, parentEntity, position, rotation, tag, onLoaded));
            return entityID;
        }

        public Guid LoadAudioEntity(BaseEntity parentEntity, Vector3 position, Quaternion rotation,
            Guid? id = null, string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadAudioEntity(entityID, parentEntity, position, rotation, tag, onLoaded));
            return entityID;
        }

        public Guid LoadAutomobileEntity(BaseEntity parentEntity, Vector3 position, Quaternion rotation,
            GameObject meshPrefab, Dictionary<string, float> wheels, float mass,
            AutomobileEntityType type, Guid? id = null, string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadAutomobileEntity(entityID, parentEntity, meshPrefab, position, rotation,
                tag, wheels, mass, type, onLoaded));
            return entityID;
        }

        public Guid LoadAirplaneEntity(BaseEntity parentEntity, Vector3 position, Quaternion rotation,
            GameObject meshPrefab, float mass, Guid? id = null, string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadAirplaneEntity(entityID, parentEntity, meshPrefab, position, rotation,
                tag, mass, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Registers an existing entity component into the EntityManager.
        /// Use this when an entity component has already been added to a GameObject (e.g., from glTF import)
        /// and needs to be tracked in the entity manager's dictionary for JavaScript API access and world save/load.
        /// </summary>
        /// <param name="entity">The entity to register.</param>
        /// <param name="id">The ID to register the entity under.</param>
        public void RegisterEntity(BaseEntity entity, Guid id)
        {
            if (entity == null)
            {
                LogSystem.LogError("[EntityManager->RegisterEntity] Cannot register null entity.");
                return;
            }

            if (entities.ContainsKey(id))
            {
                LogSystem.LogWarning($"[EntityManager->RegisterEntity] Entity with ID {id} already exists. Overwriting.");
                entities[id] = entity;
            }
            else
            {
                entities.Add(id, entity);
            }
        }

        /// <summary>
        /// Load a canvas entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the canvas entity.</param>
        /// <param name="position">Position to apply to the canvas entity.</param>
        /// <param name="rotation">Rotation to apply to the canvas entity.</param>
        /// <param name="scale">Scale/size to apply to the canvas entity.</param>
        /// <param name="id">ID to apply to the canvas entity.</param>
        /// <param name="isSize">Whether or not the scale value is for a size.</param>
        /// <param name="tag">Tag to apply to the canvas entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new canvas entity.</returns>
        public Guid LoadCanvasEntity(BaseEntity parentEntity,
            Vector3 position, Quaternion rotation, Vector3 scale,
            Guid? id = null, bool isSize = false,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadCanvasEntity(entityID, parentEntity,
                position, rotation, scale, isSize, tag, onLoaded));

            return entityID;
        }

        /// <summary>
        /// Load an HTML entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the HTML entity.</param>
        /// <param name="position">Position to apply to the HTML entity.</param>
        /// <param name="rotation">Rotation to apply to the HTML entity.</param>
        /// <param name="scale">Scale/size to apply to the HTML entity.</param>
        /// <param name="id">ID to apply to the HTML entity.</param>
        /// <param name="isSize">Whether or not the scale value is for a size.</param>
        /// <param name="tag">Tag to apply to the HTML entity.</param>
        /// <param name="onMessage">Action to invoke upon receiving a world message.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new HTML entity.</returns>
        public Guid LoadHTMLEntity(BaseEntity parentEntity,
            Vector3 position, Quaternion rotation, Vector3 scale,
            Guid? id = null, bool isSize = false,
            string tag = null, Action<string> onMessage = null,
            Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadHTMLEntity(entityID, parentEntity,
                position, rotation, scale, isSize, tag, onMessage, onLoaded));

            return entityID;
        }

        /// <summary>
        /// Load an HTML UI element entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the HTML entity.</param>
        /// <param name="positionPercent">Position of the text entity as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size of the text entity as a percentage of the canvas.</param>
        /// <param name="id">ID to apply to the HTML entity.</param>
        /// <param name="tag">Tag to apply to the HTML entity.</param>
        /// <param name="onMessage">Action to invoke upon receiving a world message.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new HTML entity.</returns>
        public Guid LoadHTMLUIElementEntity(CanvasEntity parentEntity,
            Vector2 positionPercent, Vector2 sizePercent,
            Guid? id = null, string tag = null, Action<string> onMessage = null,
            Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadHTMLUIElementEntity(entityID, parentEntity,
                positionPercent, sizePercent, tag, onMessage, onLoaded));

            return entityID;
        }

        /// <summary>
        /// Load a text entity.
        /// </summary>
        /// <param name="text">Text to place in the text entity.</param>
        /// <param name="fontSize">Font size to apply to the text entity.</param>
        /// <param name="parentEntity">Parent entity to give the text entity.</param>
        /// <param name="positionPercent">Position to apply to the text entity
        /// as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size to apply to the text entity
        /// as a percentage of the canvas.</param>
        /// <param name="id">ID to apply to the text entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <param name="tag">Tag to apply to the text entity.</param>
        /// <returns>The ID of the new text entity.</returns>
        public Guid LoadTextEntity(string text, int fontSize, UIEntity parentEntity,
            Vector2 positionPercent, Vector2 sizePercent, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadTextEntity(text, fontSize, entityID,
                parentEntity, positionPercent, sizePercent, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load a button entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the button entity.</param>
        /// <param name="positionPercent">Position to apply to the button entity
        /// as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size to apply to the button entity
        /// as a percentage of the canvas.</param>
        /// <param name="onClick">Action to perform on click of the button.</param>
        /// <param name="id">ID to apply to the button entity.</param>
        /// <param name="tag">Tag to apply to the button entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new button entity.</returns>
        public Guid LoadButtonEntity(CanvasEntity parentEntity,
            Vector2 positionPercent, Vector2 sizePercent, Action onClick, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadButtonEntity(entityID,
                parentEntity, positionPercent, sizePercent, onClick, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load a dropdown entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the dropdown entity.</param>
        /// <param name="positionPercent">Position to apply to the dropdown entity
        /// as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size to apply to the dropdown entity
        /// as a percentage of the canvas.</param>
        /// <param name="onChange">Action to perform on change of the dropdown.</param>
        /// <param name="options">Options to apply to the dropdown</param>
        /// <param name="id">ID to apply to the dropdown entity.</param>
        /// <param name="tag">Tag to apply to the dropdown entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new dropdown entity.</returns>
        public Guid LoadDropdownEntity(CanvasEntity parentEntity,
            Vector2 positionPercent, Vector2 sizePercent, Action<int> onChange,
            List<string> options, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadDropdownEntity(entityID,
                parentEntity, positionPercent, sizePercent, onChange, options, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load an input entity.
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the input entity.</param>
        /// <param name="positionPercent">Position to apply to the input entity
        /// as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size to apply to the input entity
        /// as a percentage of the canvas.</param>
        /// <param name="id">ID to apply to the input entity.</param>
        /// <param name="tag">Tag to apply to the input entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new input entity.</returns>
        public Guid LoadInputEntity(CanvasEntity parentEntity,
            Vector2 positionPercent, Vector2 sizePercent, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadInputEntity(entityID,
                parentEntity, positionPercent, sizePercent, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Load an image entity.
        /// </summary>
        /// <param name="texture">Texture to place on the image entity.</param>
        /// <param name="parentEntity">Parent entity to give the image entity.</param>
        /// <param name="positionPercent">Position to apply to the image entity
        /// as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size to apply to the image entity
        /// as a percentage of the canvas.</param>
        /// <param name="id">ID to apply to the image entity.</param>
        /// <param name="tag">Tag to apply to the image entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new image entity.</returns>
        public Guid LoadImageEntity(Texture texture, UIEntity parentEntity,
            Vector2 positionPercent, Vector2 sizePercent, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadImageEntity(texture, entityID,
                parentEntity, positionPercent, sizePercent, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentEntity">Parent entity to give the voxel entity.</param>
        /// <param name="position">Position to apply to the voxel entity.</param>
        /// <param name="rotation">Rotation to apply to the voxel entity.</param>
        /// <param name="scale">Scale to apply to the voxel entity.</param>
        /// <param name="id">ID to apply to the voxel entity.</param>
        /// <param name="tag">Tag to apply to the voxel entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>The ID of the new voxel entity.</returns>
        public Guid LoadVoxelEntity(BaseEntity parentEntity,
            Vector3 position, Quaternion rotation, Vector3 scale, Guid? id = null,
            string tag = null, Action onLoaded = null)
        {
            Guid entityID = id.HasValue ? id.Value : GetEntityID();
            StartCoroutine(LoadVoxelEntity(entityID,
                parentEntity, position, rotation, scale, tag, onLoaded));
            return entityID;
        }

        /// <summary>
        /// Loads the default character entity model.
        /// </summary>
        /// <param name="id">ID of the character entity.</param>
        /// <param name="tag">Tag of the character entity.</param>
        /// <param name="parent">Parent of the character entity.</param>
        /// <param name="position">Position of the character entity.</param>
        /// <param name="rotation">Rotation of the character entity.</param>
        /// <param name="scale">Scale/size of the character entity.</param>
        /// <param name="isSize">Whether or not the scale value is for a size.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <param name="radius">Radius of the character entity model.</param>
        /// <param name="height">Height of the character entity model.</param>
        /// <param name="center">Center of the character entity model.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadDefaultCharacterEntity(Guid id, string tag, BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale, bool isSize, Action onLoaded,
            float radius = 0.22f, float height = 1.69f, float center = 0.854f)
        {
            GameObject characterEntityObject = new GameObject();
            characterEntityObject.name = "CharacterEntity-" + id.ToString();
            CharacterEntity entity = characterEntityObject.AddComponent<CharacterEntity>();
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);
            if (isSize)
            {
                entity.SetSize(scale);
            }
            else
            {
                entity.SetScale(scale);
            }
            entity.center = center;
            entity.radius = radius;
            entity.height = height;
            entity.Initialize(id, null, Vector3.zero, Quaternion.identity, Vector3.zero);
            entity.entityTag = tag == null ? "" : tag;

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }
            
            yield return null;
        }

        /// <summary>
        /// Loads the default character entity model.
        /// </summary>
        /// <param name="id">ID of the character entity.</param>
        /// <param name="tag">Tag of the character entity.</param>
        /// <param name="parent">Parent of the character entity.</param>
        /// <param name="meshPrefab">Prefab to load mesh character entity from.</param>
        /// <param name="meshOffset">Offset for the mesh character entity object.</param>
        /// <param name="meshRotation">Rotation for the mesh character entity object.</param>
        /// <param name="avatarLabelOffset">Offset for the mesh character entity's label.</param>
        /// <param name="position">Position of the character entity.</param>
        /// <param name="rotation">Rotation of the character entity.</param>
        /// <param name="scale">Scale/size of the character entity.</param>
        /// <param name="isSize">Whether or not the scale value is for a size.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <param name="radius">Radius of the character entity model.</param>
        /// <param name="height">Height of the character entity model.</param>
        /// <param name="center">Center of the character entity model.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadMeshCharacterEntity(Guid id, string tag, BaseEntity parent,
            GameObject meshPrefab, Vector3 meshOffset, Quaternion meshRotation, Vector3 avatarLabelOffset,
            Vector3 position, Quaternion rotation, Vector3 scale, bool isSize, Action onLoaded,
            float radius = 0.22f, float height = 1.69f, float center = 0.854f)
        {
            GameObject characterEntityObject = new GameObject();
            characterEntityObject.name = "CharacterEntity-" + id.ToString();
            CharacterEntity entity = characterEntityObject.AddComponent<CharacterEntity>();
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);
            if (isSize)
            {
                entity.SetSize(scale);
            }
            else
            {
                entity.SetScale(scale);
            }
            entity.center = center;
            entity.radius = radius;
            entity.height = height;
            entity.Initialize(id, meshPrefab, meshOffset, meshRotation, avatarLabelOffset);
            entity.entityTag = tag == null ? "" : tag;

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads an empty entity.
        /// </summary>
        /// <param name="id">ID of the empty entity.</param>
        /// <param name="parent">Parent of the empty entity.</param>
        /// <param name="position">Position of the empty entity.</param>
        /// <param name="rotation">Rotation of the empty entity.</param>
        /// <param name="scale">Scale/size of the empty entity.</param>
        /// <param name="isSize">Whether or not the scale value is for a size.</param>
        /// <param name="tag">Tag of the empty entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadEmptyEntity(Guid id, BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale, bool isSize,
            string tag, Action onLoaded)
        {
            GameObject emptyEntityObject = new GameObject("EmptyEntity-" + id.ToString());
            ContainerEntity entity = emptyEntityObject.AddComponent<ContainerEntity>();
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);
            if (isSize)
            {
                entity.SetSize(scale);
            }
            else
            {
                entity.SetScale(scale);
            }
            entity.Initialize(id);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a light entity.
        /// </summary>
        /// <param name="id">ID of the light entity.</param>
        /// <param name="parent">Parent of the light entity.</param>
        /// <param name="position">Position of the light entity.</param>
        /// <param name="rotation">Rotation of the light entity.</param>
        /// <param name="tag">Tag of the light entity</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadLightEntity(Guid id, BaseEntity parent,
            Vector3 position, Quaternion rotation, string tag, Action onLoaded)
        {
            GameObject lightEntityObject = new GameObject("LightEntity-" + id.ToString());
            LightEntity entity = lightEntityObject.AddComponent<LightEntity>();
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);
            entity.Initialize(id);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a mesh entity.
        /// </summary>
        /// <param name="meshPrefab">Prefab to load the mesh entity from.</param>
        /// <param name="id">ID of the mesh entity.</param>
        /// <param name="parent">Parent of the mesh entity.</param>
        /// <param name="position">Position of the mesh entity.</param>
        /// <param name="rotation">Rotation of the mesh entity.</param>
        /// <param name="tag">Tag of the mesh entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the OnLoaded action.</returns>
        private System.Collections.IEnumerator LoadMeshEntity(GameObject meshPrefab, Guid id, BaseEntity parent,
            Vector3 position, Quaternion rotation, string tag, Action onLoaded)
        {
            GameObject meshGO = Instantiate(meshPrefab);
            meshGO.name = "MeshEntity-" + id.ToString();
            MeshEntity entity = meshGO.AddComponent<MeshEntity>();
            if (entities.ContainsKey(id))
            {
                LogSystem.LogWarning("[EntityManager->LoadMeshEntity] Entity with ID " + id.ToString() + " already exists. Overwriting.");
                entities[id] = entity;
            }
            else
            {
                entities.Add(id, entity);
            }
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);
            entity.Initialize(id);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a terrain entity.
        /// </summary>
        /// <param name="length">Length of the terrain.</param>
        /// <param name="width">Width of the terrain.</param>
        /// <param name="height">Height of the terrain.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="id">ID of the terrain entity.</param>
        /// <param name="parent">Parent of the terrain entity.</param>
        /// <param name="position">Position of the terrain entity.</param>
        /// <param name="rotation">Rotation of the terrain entity.</param>
        /// <param name="tag">Tag of the terrain entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadTerrainEntity(float length, float width, float height,
            float[,] heights, Terrain.TerrainEntityLayer[] layers, Dictionary<int, float[,]> layerMasks, Guid id,
            BaseEntity parent, Vector3 position, Quaternion rotation, bool stitchTerrain, string tag, Action onLoaded)
        {
            TerrainEntity entity = TerrainEntity.Create(length, width, height, heights, layers, layerMasks, id, stitchTerrain);
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a hybrid terrain entity.
        /// </summary>
        /// <param name="length">Length of the terrain.</param>
        /// <param name="width">Width of the terrain.</param>
        /// <param name="height">Height of the terrain.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="id">ID of the terrain entity.</param>
        /// <param name="parent">Parent of the terrain entity.</param>
        /// <param name="position">Position of the terrain entity.</param>
        /// <param name="rotation">Rotation of the terrain entity.</param>
        /// <param name="tag">Tag of the terrain entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadHybridTerrainEntity(float length, float width, float height,
            float[,] heights, Terrain.TerrainEntityLayer[] layers, Dictionary<int, float[,]> layerMasks, Guid id,
            BaseEntity parent, Vector3 position, Quaternion rotation, bool stitchTerrains, string tag, Action onLoaded)
        {
            HybridTerrainEntity entity = HybridTerrainEntity.Create(
                length, width, height, heights, layers, layerMasks, id, stitchTerrains);
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a water body entity.
        /// </summary>
        /// <param name="shallowColor">Color for the shallow zone.</param>
        /// <param name="deepColor">Color for the deep zone.</param>
        /// <param name="specularColor">Specular color.</param>
        /// <param name="scatteringColor">Scattering color.</param>
        /// <param name="deepStart">Start of deep zone.</param>
        /// <param name="deepEnd">End of deep zone.</param>
        /// <param name="distortion">Distortion factor (range 0-128).</param>
        /// <param name="smoothness">Smoothness factor (range 0-1).</param>
        /// <param name="numWaves">Number of waves (range 1-32).</param>
        /// <param name="waveAmplitude">Wave amplitude (range 0-1).</param>
        /// <param name="waveSteepness">Wave steepness (range 0-1).</param>
        /// <param name="waveSpeed">Wave speed.</param>
        /// <param name="waveLength">Wave length.</param>
        /// <param name="scale">Scale of the waves.</param>
        /// <param name="intensity">Intensity factor (range 0-1).</param>
        /// <param name="id">ID of the water body entity.</param>
        /// <param name="parent">Parent of the water body entity.</param>
        /// <param name="position">Position of the water body entity.</param>
        /// <param name="rotation">Rotation of the water body entity.</param>
        /// <param name="tag">Tag of the water body entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadWaterBodyEntity(Color shallowColor, Color deepColor,
            Color specularColor, Color scatteringColor, float deepStart, float deepEnd, float distortion,
            float smoothness, float numWaves, float waveAmplitude, float waveSteepness, float waveSpeed,
            float waveLength, float scale, float intensity, Guid id, BaseEntity parent,
            Vector3 position, Quaternion rotation, string tag, Action onLoaded)
        {
            WaterBodyEntity entity = WaterBodyEntity.Create(shallowColor, deepColor, specularColor,
                scatteringColor, deepStart, deepEnd, distortion, smoothness, numWaves, waveAmplitude,
                waveSteepness, waveSpeed, waveLength, scale, intensity, id);
            
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a water blocker entity.
        /// </summary>
        /// <param name="id">ID of the water blocker entity.</param>
        /// <param name="parent">Parent of the water blocker entity.</param>
        /// <param name="position">Position of the water blocker entity.</param>
        /// <param name="rotation">Rotation of the water blocker entity.</param>
        /// <param name="tag">Tag of the water blocker entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadWaterBlockerEntity(Guid id,
        BaseEntity parent, Vector3 position, Quaternion rotation, string tag, Action onLoaded)
        {
            WaterBlockerEntity entity = WaterBlockerEntity.Create(id);
            
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        private System.Collections.IEnumerator LoadAudioEntity(Guid id, BaseEntity parent, Vector3 position, Quaternion rotation,
            string tag, Action onLoaded)
        {
            GameObject audioEntityObject = new GameObject("AudioEntity-" + id.ToString());
            AudioEntity entity = audioEntityObject.AddComponent<AudioEntity>();
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);
            entity.Initialize(id);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        private System.Collections.IEnumerator LoadAutomobileEntity(Guid id, BaseEntity parent,
            GameObject meshPrefab, Vector3 position, Quaternion rotation, string tag,
            Dictionary<string, float> wheels, float mass, AutomobileEntityType type, Action onLoaded)
        {
            GameObject automobileGO = Instantiate(meshPrefab);
            automobileGO.name = "AutomobileEntity-" + id.ToString();
            AutomobileEntity entity = automobileGO.AddComponent<AutomobileEntity>();
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);

            Dictionary<GameObject, float> convertedWheels = new Dictionary<GameObject, float>();
            foreach (KeyValuePair<string, float> wheel in wheels)
            {
                GameObject wheelSubMesh = FindChildObjectByName(automobileGO, wheel.Key);
                if (wheelSubMesh == null)
                {
                    LogSystem.LogWarning("[EntityManager->LoadAutomobileEntity] Unable to find wheel submesh "
                        + wheel.Key);
                }
                else
                {
                    convertedWheels.Add(wheelSubMesh, wheel.Value);
                }
            }

            entity.Initialize(id, convertedWheels, mass, type);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        private System.Collections.IEnumerator LoadAirplaneEntity(Guid id, BaseEntity parent,
            GameObject meshPrefab, Vector3 position, Quaternion rotation, string tag, float mass, Action onLoaded)
        {
            GameObject airplaneMeshGO = Instantiate(meshPrefab);
            GameObject airplaneGO = Instantiate(airplaneEntityPrefab);
            airplaneGO.name = "AirplaneEntity-" + id.ToString();
            AirplaneEntity entity = airplaneGO.AddComponent<AirplaneEntity>();
            entities.Add(id, entity);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);
            entity.Initialize(id, airplaneMeshGO, mass);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a canvas entity.
        /// </summary>
        /// <param name="id">ID of the canvas entity.</param>
        /// <param name="parent">Parent of the canvas entity.</param>
        /// <param name="position">Position of the canvas entity.</param>
        /// <param name="rotation">Rotation of the canvas entity.</param>
        /// <param name="scale">Scale/size of the canvas entity.</param>
        /// <param name="isSize">Whether or not the scale value is for a size.</param>
        /// <param name="tag">Tag of the canvas entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadCanvasEntity(Guid id, BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale, bool isSize,
            string tag, Action onLoaded)
        {
            GameObject canvasEntityObject = new GameObject("CanvasEntity-" + id.ToString());
            CanvasEntity entity = canvasEntityObject.AddComponent<CanvasEntity>();
            entities.Add(id, entity);
            entity.Initialize(id);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);

            if (isSize)
            {
                entity.SetSize(scale);
            }
            else
            {
                entity.SetScale(scale);
            }

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads an HTML entity.
        /// </summary>
        /// <param name="id">ID of the HTML entity.</param>
        /// <param name="parent">Parent of the HTML entity.</param>
        /// <param name="position">Position of the HTML entity.</param>
        /// <param name="rotation">Rotation of the HTML entity.</param>
        /// <param name="scale">Scale/size of the HTML entity.</param>
        /// <param name="isSize">Whether or not the scale value is for a size.</param>
        /// <param name="tag">Tag of the HTML entity.</param>
        /// <param name="onMessage">Action to invoke upon receiving a world message.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadHTMLEntity(Guid id, BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale, bool isSize,
            string tag, Action<string> onMessage, Action onLoaded)
        {
            GameObject htmlEntityObject = new GameObject("HTMLEntity-" + id.ToString());
            HTMLEntity entity = htmlEntityObject.AddComponent<HTMLEntity>();
            entities.Add(id, entity);
            entity.Initialize(id);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.onWorldMessage = onMessage;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);

            if (isSize)
            {
                entity.SetSize(scale);
            }
            else
            {
                entity.SetScale(scale);
            }

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads an HTML UI element entity.
        /// </summary>
        /// <param name="id">ID of the HTML entity.</param>
        /// <param name="parent">Parent of the HTML entity.</param>
        /// <param name="positionPercent">Position of the text entity as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size of the text entity as a percentage of the canvas.</param>
        /// <param name="tag">Tag of the HTML entity.</param>
        /// <param name="onMessage">Action to invoke upon receiving a world message.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadHTMLUIElementEntity(Guid id, CanvasEntity parent,
            Vector2 positionPercent, Vector2 sizePercent, string tag, Action<string> onMessage, Action onLoaded)
        {
            GameObject htmlEntityObject = new GameObject("HTMLEntity-" + id.ToString());
            HTMLUIElementEntity entity = htmlEntityObject.AddComponent<HTMLUIElementEntity>();
            entities.Add(id, entity);
            entity.Initialize(id, parent);
            entity.entityTag = tag;
            entity.onWorldMessage = onMessage;
            if (parent != null)
            {
                entity.SetPositionPercent(positionPercent, true);
                entity.SetSizePercent(sizePercent, true);
            }

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a text entity.
        /// </summary>
        /// <param name="text">Text of the text entity.</param>
        /// <param name="fontSize">Font size of the text entity.</param>
        /// <param name="id">ID of the text entity.</param>
        /// <param name="parent">Parent of the text entity.</param>
        /// <param name="positionPercent">Position of the text entity as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size of the text entity as a percentage of the canvas.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <param name="tag">Tag of the text entity.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadTextEntity(string text, int fontSize,
            Guid id, UIEntity parent, Vector2 positionPercent, Vector2 sizePercent,
            string tag, Action onLoaded)
        {
            GameObject textEntityObject = new GameObject("TextEntity-" + id.ToString());
            TextEntity entity = textEntityObject.AddComponent<TextEntity>();
            entities.Add(id, entity);
            entity.Initialize(id, parent);
            entity.entityTag = tag;
            if (parent != null)
            {
                entity.SetPositionPercent(positionPercent, true);
                entity.SetSizePercent(sizePercent, true);
            }
            entity.SetText(text);
            entity.SetFontSize(fontSize);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a button entity.
        /// </summary>
        /// <param name="id">ID of the button entity.</param>
        /// <param name="parent">Parent of the button entity.</param>
        /// <param name="positionPercent">Position of the button entity as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size of the button entity as a percentage of the canvas.</param>
        /// <param name="onClick">Action to perform on click of the button.</param>
        /// <param name="tag">Tag of the button entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadButtonEntity(Guid id, CanvasEntity parent,
            Vector2 positionPercent, Vector2 sizePercent, Action onClick, string tag, Action onLoaded)
        {
            GameObject buttonEntityObject = new GameObject("ButtonEntity-" + id.ToString());
            ButtonEntity entity = buttonEntityObject.AddComponent<ButtonEntity>();
            entities.Add(id, entity);
            entity.Initialize(id, parent);
            entity.entityTag = tag;
            if (parent != null)
            {
                entity.SetPositionPercent(positionPercent, true);
                entity.SetSizePercent(sizePercent, true);
            }

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            entity.SetOnClick(onClick);

            yield return null;
        }

        /// <summary>
        /// Loads a dropdown entity.
        /// </summary>
        /// <param name="id">ID of the dropdown entity.</param>
        /// <param name="parent">Parent of the dropdown entity.</param>
        /// <param name="positionPercent">Position of the dropdown entity as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size of the dropdown entity as a percentage of the canvas.</param>
        /// <param name="onChange">Action to perform on change of the dropdown.</param>
        /// <param name="options">Options to apply to the dropdown</param>
        /// <param name="tag">Tag of the dropdown entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadDropdownEntity(Guid id, CanvasEntity parent,
            Vector2 positionPercent, Vector2 sizePercent, Action<int> onChange, List<string> options,
            string tag, Action onLoaded)
        {
            GameObject dropdownEntityObject = new GameObject("DropdownEntity-" + id.ToString());
            DropdownEntity entity = dropdownEntityObject.AddComponent<DropdownEntity>();
            entities.Add(id, entity);
            entity.Initialize(id, parent);
            entity.entityTag = tag;
            if (parent != null)
            {
                entity.SetPositionPercent(positionPercent, true);
                entity.SetSizePercent(sizePercent, true);
            }

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            entity.SetOnChange(onChange);
            if (options != null)
            {
                foreach (string option in options)
                {
                    entity.AddOption(option);
                }
            }

            yield return null;
        }

        /// <summary>
        /// Loads an input entity.
        /// </summary>
        /// <param name="id">ID of the input entity.</param>
        /// <param name="parent">Parent of the input entity.</param>
        /// <param name="positionPercent">Position of the input entity as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size of the input entity as a percentage of the canvas.</param>
        /// <param name="tag">Tag of the input entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadInputEntity(Guid id, CanvasEntity parent,
            Vector2 positionPercent, Vector2 sizePercent, string tag, Action onLoaded)
        {
            GameObject inputEntityObject = Instantiate(inputEntityPrefab);
            inputEntityObject.name = "InputEntity-" + id.ToString();
            InputEntity entity = inputEntityObject.AddComponent<InputEntity>();
            entities.Add(id, entity);
            entity.Initialize(id, parent);
            entity.entityTag = tag;
            if (parent != null)
            {
                entity.SetPositionPercent(positionPercent, true);
                entity.SetSizePercent(sizePercent, true);
            }

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a image entity.
        /// </summary>
        /// <param name="image">Image to place on the image entity.</param>
        /// <param name="id">ID of the text entity.</param>
        /// <param name="parent">Parent of the text entity.</param>
        /// <param name="positionPercent">Position of the text entity as a percentage of the canvas.</param>
        /// <param name="sizePercent">Size of the text entity as a percentage of the canvas.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <param name="tag">Tag of the text entity.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadImageEntity(Texture image,
            Guid id, UIEntity parent, Vector2 positionPercent, Vector2 sizePercent,
            string tag, Action onLoaded)
        {
            GameObject imageEntityObject = new GameObject("ImageEntity-" + id.ToString());
            ImageEntity entity = imageEntityObject.AddComponent<ImageEntity>();
            entities.Add(id, entity);
            entity.Initialize(id, parent);
            entity.entityTag = tag;
            if (parent != null)
            {
                entity.SetPositionPercent(positionPercent, true);
                entity.SetSizePercent(sizePercent, true);
            }
            entity.SetTexture(image);

            if (onLoaded != null)
            {
                onLoaded.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Loads a voxel entity.
        /// </summary>
        /// <param name="id">ID of the voxel entity.</param>
        /// <param name="parent">Parent of the voxel entity.</param>
        /// <param name="position">Position of the voxel entity.</param>
        /// <param name="rotation">Rotation of the voxel entity.</param>
        /// <param name="scale">Scale of the voxel entity.</param>
        /// <param name="tag">Tag of the voxel entity.</param>
        /// <param name="onLoaded">Action to perform when loading is complete.</param>
        /// <returns>Coroutine, completes after invocation of the onLoaded action.</returns>
        private System.Collections.IEnumerator LoadVoxelEntity(Guid id, BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale, string tag, Action onLoaded)
        {
            GameObject voxelEntityObject = new GameObject("VoxelEntity-" + id.ToString());
            VoxelEntity entity = voxelEntityObject.AddComponent<VoxelEntity>();
            entities.Add(id, entity);
            entity.Initialize(id);
            entity.SetParent(parent);
            entity.entityTag = tag;
            entity.SetPosition(position, true);
            entity.SetRotation(rotation, true);
            entity.SetScale(scale);

            if (onLoaded != null)
            {
                yield return new WaitForSeconds(0.01f);
                onLoaded.Invoke();
            }

            yield return null;
        }
        
        /// <summary>
        /// Check if entity with given ID exists.
        /// </summary>
        /// <param name="id">ID of entity to check.</param>
        /// <returns>Whether or not entity exists.</returns>
        public bool Exists(Guid id)
        {
            return entities.ContainsKey(id);
        }
        
        /// <summary>
        /// Find entity with given ID.
        /// </summary>
        /// <param name="id">ID of entity to find.</param>
        /// <returns>Reference to the found entity.</returns>
        public BaseEntity FindEntity(Guid id)
        {
            if (!entities.ContainsKey(id))
            {
                return null;
            }

            return entities[id];
        }

        /// <summary>
        /// Find entity with given tag.
        /// </summary>
        /// <param name="tag">Tag of entity to get.</param>
        /// <returns>Entity with given tag.</returns>
        public BaseEntity FindEntityByTag(string tag)
        {
            foreach (BaseEntity entity in GetAllEntities())
            {
                if (entity.entityTag == tag)
                {
                    return entity;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>Array of entities.</returns>
        public BaseEntity[] GetAllEntities()
        {
            BaseEntity[] allEntities = new BaseEntity[entities.Count];
            entities.Values.CopyTo(allEntities, 0);
            return allEntities;
        }

        /// <summary>
        /// Get all top-level entities.
        /// </summary>
        /// <returns>Array of all top level entities.</returns>
        public BaseEntity[] GetAllTopLevelEntities()
        {
            List<BaseEntity> topLevelEntities = new List<BaseEntity>();
            foreach (BaseEntity entity in entities.Values)
            {
                if (entity == null)
                {
                    continue;
                }

                if (entity.GetParent() == null)
                {
                    topLevelEntities.Add(entity);
                }
            }
            return topLevelEntities.ToArray();
        }

        /// <summary>
        /// Unload all entities.
        /// </summary>
        public void Unload()
        {
            foreach (BaseEntity entity in entities.Values)
            {
                if (entity != null)
                {
                    entity.Delete(false);
                }
            }
            entities.Clear();
        }

        private GameObject FindChildObjectByName(GameObject parentObject, string childName)
        {
            Transform[] transforms = parentObject.GetComponentsInChildren<Transform>();
            foreach (Transform t in transforms)
            {
                if (t.name == childName)
                {
                    return t.gameObject;
                }
            }
            return null;
        }
    }
}