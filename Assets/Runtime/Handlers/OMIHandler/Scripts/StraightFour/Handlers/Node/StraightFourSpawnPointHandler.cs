// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.SpawnPoint;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_spawn_point.
    /// Registers spawn points with the SpawnPointRegistry.
    /// </summary>
    public class StraightFourSpawnPointHandler : StraightFourNodeHandlerBase<OMISpawnPointNode>
    {
        public override string ExtensionName => OMISpawnPointExtension.ExtensionName;
        public override int Priority => 50;

        public override Task OnNodeImportAsync(OMISpawnPointNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null)
            {
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing spawn point for node {nodeIndex}: {data.Title ?? targetObject.name}");

            // Get spawn point registry from context
            SpawnPointRegistry registry = null;
            if (context.CustomData.TryGetValue(StraightFourCustomDataKeys.SpawnPointRegistry, out var regObj))
            {
                registry = regObj as SpawnPointRegistry;
            }
            
            if (registry == null)
            {
                Logging.LogWarning("[StraightFour] No SpawnPointRegistry available in context");
                return Task.CompletedTask;
            }

            // Register with the existing SpawnPointRegistry
            registry.Register(
                targetObject.transform.position,
                targetObject.transform.rotation,
                data.Title ?? targetObject.name,
                data.Team,
                data.Group
            );

            // Add visual indicator component if verbose logging enabled
            if (context.Settings.VerboseLogging)
            {
                var indicator = targetObject.AddComponent<SpawnPointIndicator>();
                indicator.title = data.Title ?? targetObject.name;
            }

            Logging.Log($"[StraightFour] Registered spawn point: {data.Title ?? targetObject.name} at {targetObject.transform.position}");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Visual indicator for spawn points (debug only).
    /// </summary>
    public class SpawnPointIndicator : MonoBehaviour
    {
        public string title;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
        }
    }
}
#endif
