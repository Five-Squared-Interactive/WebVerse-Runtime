// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.Link;
using UnityEngine;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_link.
    /// Creates link/portal components for world traversal.
    /// </summary>
    public class StraightFourLinkHandler : StraightFourNodeHandlerBase<OMILinkNode>
    {
        public override string ExtensionName => OMILinkExtension.ExtensionName;
        public override int Priority => 50;

        public override Task OnNodeImportAsync(OMILinkNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null)
            {
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing link for node {nodeIndex}: {data.Title ?? data.Uri}");

            // Add link component
            var linkComponent = targetObject.AddComponent<OMILinkBehavior>();
            linkComponent.Initialize(data, GetRuntime(context));

            // Find parent entity using glTF parent node index
            BaseEntity parentEntity = null;
            if (context.CustomData != null && context.CustomData.TryGetValue("SF_NodeParentIndices", out var parentMapObj))
            {
                var parentMap = parentMapObj as Dictionary<int, int>;
                if (parentMap != null && parentMap.TryGetValue(nodeIndex, out var parentNodeIndex))
                {
                    parentEntity = GetEntityForNode(context, parentNodeIndex);
                }
            }
            // Create entity for the link with correct parent
            GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);

            // Add collider if not present (for interaction)
            if (targetObject.GetComponent<Collider>() == null)
            {
                var boxCollider = targetObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                boxCollider.size = Vector3.one; // Default size
            }

            Logging.Log($"[StraightFour] Created link: {data.Title ?? "Untitled"} -> {data.Uri}");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Behavior component for OMI link/portal functionality.
    /// </summary>
    public class OMILinkBehavior : MonoBehaviour
    {
        /// <summary>
        /// The target URI for this link.
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        /// Optional title for this link.
        /// </summary>
        public string Title { get; private set; }

        private WebVerseRuntime runtime;

        public void Initialize(OMILinkNode data, WebVerseRuntime runtime)
        {
            Uri = data.Uri;
            Title = data.Title;
            this.runtime = runtime;
        }

        /// <summary>
        /// Activate the link (navigate to the destination).
        /// </summary>
        public void Activate()
        {
            if (string.IsNullOrEmpty(Uri))
            {
                Logging.LogWarning("[OMILink] Cannot activate link with empty URI");
                return;
            }

            Logging.Log($"[OMILink] Activating link: {Title ?? "Untitled"} -> {Uri}");

            // Handle different URI types
            if (Uri.StartsWith("#"))
            {
                // Fragment - navigate to spawn point within current world
                HandleFragmentLink(Uri.Substring(1));
            }
            else if (Uri.StartsWith("./") || Uri.StartsWith("../"))
            {
                // Relative path - load relative world
                HandleRelativeLink(Uri);
            }
            else
            {
                // Absolute URL - load world from URL
                HandleAbsoluteLink(Uri);
            }
        }

        private void HandleFragmentLink(string fragment)
        {
            // Find spawn point with matching ID
            Logging.Log($"[OMILink] Navigating to spawn point: {fragment}");
            // TODO: Implement spawn point navigation via runtime
        }

        private void HandleRelativeLink(string relativePath)
        {
            Logging.Log($"[OMILink] Loading relative world: {relativePath}");
            // TODO: Resolve relative path and load via runtime
        }

        private void HandleAbsoluteLink(string url)
        {
            Logging.Log($"[OMILink] Loading world from URL: {url}");
            
            if (runtime != null)
            {
                // Use WebVerse runtime to load new world
                runtime.LoadWorld(url, (result) =>
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        Logging.Log($"[OMILink] World loaded successfully from: {url}");
                    }
                    else
                    {
                        Logging.LogWarning($"[OMILink] Failed to load world from {url}: {result}");
                    }
                });
            }
            else
            {
                Logging.LogWarning("[OMILink] No runtime available for world loading");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the entering object is the player
            if (other.CompareTag("Player"))
            {
                Activate();
            }
        }
    }
}
#endif
