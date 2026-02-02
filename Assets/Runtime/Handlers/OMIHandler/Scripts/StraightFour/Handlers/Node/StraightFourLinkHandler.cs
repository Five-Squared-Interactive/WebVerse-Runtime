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
using Newtonsoft.Json.Linq;

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
            BaseEntity entity = GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);

            // Configure link on entity using adapter pattern (Task 2R.7)
            if (entity != null)
            {
                AddLinkToEntity(entity, nodeIndex, context);
            }

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

        /// <summary>
        /// Adds link to entity by creating LinkData directly from OMI JSON.
        /// No adapter - creates data struct directly.
        /// </summary>
        private void AddLinkToEntity(BaseEntity entity, int nodeIndex, OMIImportContext context)
        {
            // Get the raw glTF JSON to extract OMI_link extension data
            if (!context.CustomData.TryGetValue("SF_GltfJson", out var jsonObj))
            {
                Logging.LogWarning("[StraightFourLinkHandler] No glTF JSON found in context");
                return;
            }

            var root = jsonObj as JObject;
            if (root == null)
            {
                return;
            }

            // Navigate to the node's OMI_link extension
            var nodes = root["nodes"] as JArray;
            if (nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Count)
            {
                return;
            }

            var node = nodes[nodeIndex] as JObject;
            var extensions = node?["extensions"] as JObject;
            var omiLink = extensions?["OMI_link"] as JObject;

            if (omiLink == null)
            {
                Logging.LogWarning($"[StraightFourLinkHandler] No OMI_link extension found for node {nodeIndex}");
                return;
            }

            // Extract values from OMI JSON
            string uri = omiLink["uri"]?.Value<string>() ?? "";
            string title = omiLink["title"]?.Value<string>() ?? "";

            if (string.IsNullOrEmpty(uri))
            {
                Logging.LogWarning("[StraightFourLinkHandler] OMI_link has empty or null URI.");
                return;
            }

            // Create LinkData struct directly
            var linkData = LinkData.Create(uri, title);

            // Add link to entity using BaseEntity API
            entity.AddLink(linkData);
            Logging.Log($"[StraightFourLinkHandler] Added link to entity: {uri}");
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
