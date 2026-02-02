// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using OMI;
using Newtonsoft.Json.Linq;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour
{
    /// <summary>
    /// Helper class for detecting OMI extensions on nodes.
    /// Provides methods to check for extensions and find child nodes with specific extensions.
    /// </summary>
    public static class OMIExtensionDetector
    {
        /// <summary>
        /// Checks if a node has a specific extension.
        /// </summary>
        /// <param name="context">The OMI import context containing the glTF JSON data.</param>
        /// <param name="nodeIndex">The index of the node to check.</param>
        /// <param name="extensionName">The name of the extension to look for.</param>
        /// <returns>True if the node has the specified extension, false otherwise.</returns>
        public static bool HasExtension(OMIImportContext context, int nodeIndex, string extensionName)
        {
            if (context == null || string.IsNullOrEmpty(extensionName))
                return false;

            // Try to get the glTF JSON from context
            if (!context.CustomData.TryGetValue("SF_GltfJson", out var jsonObj))
                return false;

            var root = jsonObj as JObject;
            if (root == null)
                return false;

            var nodes = root["nodes"] as JArray;
            if (nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Count)
                return false;

            var node = nodes[nodeIndex] as JObject;
            if (node == null)
                return false;

            var extensions = node["extensions"] as JObject;
            return extensions != null && extensions.ContainsKey(extensionName);
        }

        /// <summary>
        /// Gets all extensions present on a node.
        /// </summary>
        /// <param name="context">The OMI import context containing the glTF JSON data.</param>
        /// <param name="nodeIndex">The index of the node to check.</param>
        /// <returns>List of extension names found on the node. Empty list if none found.</returns>
        public static List<string> GetNodeExtensions(OMIImportContext context, int nodeIndex)
        {
            var result = new List<string>();

            if (context == null || !context.CustomData.TryGetValue("SF_GltfJson", out var jsonObj))
                return result;

            var root = jsonObj as JObject;
            if (root == null)
                return result;

            var nodes = root["nodes"] as JArray;
            if (nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Count)
                return result;

            var node = nodes[nodeIndex] as JObject;
            if (node == null)
                return result;

            var extensions = node["extensions"] as JObject;
            if (extensions != null)
            {
                foreach (var prop in extensions.Properties())
                {
                    result.Add(prop.Name);
                }
            }

            return result;
        }

        /// <summary>
        /// Finds child nodes (recursive) with a specific extension.
        /// </summary>
        /// <param name="context">The OMI import context containing the glTF JSON data.</param>
        /// <param name="parentNodeIndex">The index of the parent node to search from.</param>
        /// <param name="extensionName">The name of the extension to look for.</param>
        /// <returns>List of node indices that have the specified extension. Empty list if none found.</returns>
        public static List<int> FindChildNodesWithExtension(
            OMIImportContext context,
            int parentNodeIndex,
            string extensionName)
        {
            var result = new List<int>();

            if (context == null || !context.CustomData.TryGetValue("SF_GltfJson", out var jsonObj))
                return result;

            var root = jsonObj as JObject;
            if (root == null)
                return result;

            var nodes = root["nodes"] as JArray;
            if (nodes == null || parentNodeIndex < 0 || parentNodeIndex >= nodes.Count)
                return result;

            var parentNode = nodes[parentNodeIndex] as JObject;
            if (parentNode == null)
                return result;

            var children = parentNode["children"] as JArray;
            if (children == null)
                return result;

            foreach (var childToken in children)
            {
                int childIndex = childToken.Value<int>();
                if (HasExtension(context, childIndex, extensionName))
                {
                    result.Add(childIndex);
                }

                // Recursively check grandchildren
                result.AddRange(FindChildNodesWithExtension(context, childIndex, extensionName));
            }

            return result;
        }
    }
}
#endif
