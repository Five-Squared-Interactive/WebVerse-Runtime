// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Base class for StraightFour OMI extension handlers.
    /// Provides common utilities for entity creation and data access.
    /// </summary>
    public abstract class StraightFourHandlerBase
    {
        /// <summary>
        /// Gets the WebVerseRuntime from context CustomData.
        /// </summary>
        protected WebVerseRuntime GetRuntime(OMIImportContext context)
        {
            if (context.CustomData.TryGetValue(StraightFourCustomDataKeys.Runtime, out var runtime))
            {
                return runtime as WebVerseRuntime;
            }
            return WebVerseRuntime.Instance;
        }

        /// <summary>
        /// Gets the SpawnPointRegistry from context CustomData.
        /// </summary>
        protected SpawnPointRegistry GetSpawnPointRegistry(OMIImportContext context)
        {
            if (context.CustomData.TryGetValue(StraightFourCustomDataKeys.SpawnPointRegistry, out var registry))
            {
                return registry as SpawnPointRegistry;
            }
            return null;
        }

        /// <summary>
        /// Gets or creates the node-to-entity mapping dictionary.
        /// </summary>
        protected Dictionary<int, BaseEntity> GetNodeToEntityMap(OMIImportContext context)
        {
            if (!context.CustomData.TryGetValue(StraightFourCustomDataKeys.NodeToEntity, out var mapObj))
            {
                var map = new Dictionary<int, BaseEntity>();
                context.CustomData[StraightFourCustomDataKeys.NodeToEntity] = map;
                return map;
            }
            return mapObj as Dictionary<int, BaseEntity>;
        }

        /// <summary>
        /// Gets an existing StraightFour entity for a node, or null if not created.
        /// </summary>
        protected BaseEntity GetEntityForNode(OMIImportContext context, int nodeIndex)
        {
            var map = GetNodeToEntityMap(context);
            return map.TryGetValue(nodeIndex, out var entity) ? entity : null;
        }

        /// <summary>
        /// Registers a StraightFour entity for a node index.
        /// </summary>
        protected void RegisterEntity(OMIImportContext context, int nodeIndex, BaseEntity entity)
        {
            var map = GetNodeToEntityMap(context);
            map[nodeIndex] = entity;
        }

        /// <summary>
        /// Creates a StraightFour entity from a GameObject if one doesn't exist.
        /// Uses StraightFourEntityFactory to create the appropriate entity type based on OMI extensions.
        /// </summary>
        /// <param name="context">Import context.</param>
        /// <param name="nodeIndex">glTF node index.</param>
        /// <param name="gameObject">The Unity GameObject for this node.</param>
        /// <param name="parent">Optional parent entity.</param>
        /// <returns>The created or existing entity.</returns>
        protected BaseEntity GetOrCreateEntity(OMIImportContext context, int nodeIndex, GameObject gameObject, BaseEntity parent = null)
        {
            Logging.Log($"[StraightFour] GetOrCreateEntity called for node {nodeIndex}: {gameObject?.name}");

            // Check if entity already exists
            var existing = GetEntityForNode(context, nodeIndex);
            if (existing != null)
            {
                Logging.Log($"[StraightFour] Entity already exists for node {nodeIndex}: {existing.GetType().Name}");
                return existing;
            }

            if (gameObject == null)
            {
                Logging.Log($"[StraightFour] GameObject is null for node {nodeIndex}");
                return null;
            }

            // USE FACTORY to create appropriate entity type
            BaseEntity entity = StraightFourEntityFactory.CreateEntityFromNode(
                gameObject,
                nodeIndex,
                context,
                parent);

            if (entity != null)
            {
                RegisterEntity(context, nodeIndex, entity);
                Logging.Log($"[StraightFour] Created and registered {entity.GetType().Name} for node {nodeIndex}: {gameObject.name}");

                if (context.Settings.VerboseLogging)
                {
                    Logging.Log($"[StraightFour] Created entity for node {nodeIndex}: {gameObject.name}");
                }
            }
            else
            {
                Logging.LogWarning($"[StraightFour] Failed to create entity for node {nodeIndex}: {gameObject.name}");
            }

            return entity;
        }

        /// <summary>
        /// Converts a float array to Vector3.
        /// </summary>
        protected Vector3 ToVector3(float[] arr, Vector3 defaultValue = default)
        {
            if (arr == null || arr.Length < 3)
                return defaultValue;
            return new Vector3(arr[0], arr[1], arr[2]);
        }

        /// <summary>
        /// Converts a float array to Quaternion.
        /// </summary>
        protected Quaternion ToQuaternion(float[] arr)
        {
            if (arr == null || arr.Length < 4)
                return Quaternion.identity;
            return new Quaternion(arr[0], arr[1], arr[2], arr[3]);
        }

        /// <summary>
        /// Converts a float array to Color.
        /// </summary>
        protected Color ToColor(float[] arr, Color defaultValue = default)
        {
            if (arr == null || arr.Length < 3)
                return defaultValue;
            float a = arr.Length >= 4 ? arr[3] : 1f;
            return new Color(arr[0], arr[1], arr[2], a);
        }

        /// <summary>
        /// Log a message if verbose logging is enabled.
        /// </summary>
        protected void LogVerbose(OMIImportContext context, string message)
        {
            if (context.Settings.VerboseLogging)
            {
                Logging.Log(message);
            }
        }
    }

    /// <summary>
    /// Base class for document-level StraightFour handlers.
    /// </summary>
    /// <typeparam name="TData">The extension data type.</typeparam>
    public abstract class StraightFourDocumentHandlerBase<TData> : StraightFourHandlerBase, IOMIDocumentExtensionHandler<TData>
        where TData : class
    {
        public abstract string ExtensionName { get; }
        public virtual int Priority => 100;

        public abstract Task OnDocumentImportAsync(TData data, OMIImportContext context, CancellationToken cancellationToken = default);

        public virtual Task<TData> OnDocumentExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            // Export not implemented by default
            return Task.FromResult<TData>(null);
        }

        public Task OnImportAsync(TData data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            return OnDocumentImportAsync(data, context, cancellationToken);
        }

        public Task<TData> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            return OnDocumentExportAsync(context, cancellationToken);
        }
    }

    /// <summary>
    /// Base class for node-level StraightFour handlers.
    /// </summary>
    /// <typeparam name="TData">The extension data type.</typeparam>
    public abstract class StraightFourNodeHandlerBase<TData> : StraightFourHandlerBase, IOMINodeExtensionHandler<TData>
        where TData : class
    {
        public abstract string ExtensionName { get; }
        public virtual int Priority => 50;

        public abstract Task OnNodeImportAsync(TData data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default);

        public virtual Task<TData> OnNodeExportAsync(GameObject sourceObject, OMIExportContext context, CancellationToken cancellationToken = default)
        {
            // Export not implemented by default
            return Task.FromResult<TData>(null);
        }

        public Task OnImportAsync(TData data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            // Node-level import is handled by OnNodeImportAsync
            return Task.CompletedTask;
        }

        public Task<TData> OnExportAsync(OMIExportContext context, CancellationToken cancellationToken = default)
        {
            // Node-level export is handled by OnNodeExportAsync
            return Task.FromResult<TData>(null);
        }
    }
}
#endif
