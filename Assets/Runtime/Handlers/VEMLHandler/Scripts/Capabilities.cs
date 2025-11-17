// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.VEML
{
    /// <summary>
    /// Class for Capabilities.
    /// </summary>
    public class Capabilities
    {
        /// <summary>
        /// Capability Support Enumeration.
        /// </summary>
        public enum CapabilitySupport
        {
            Supported = 0,
            Unsupported = 1,
            LimitedSupport = 2,
            InsufficientResources = 3
        }

        /// <summary>
        /// Get the Capability Support Value for a specific Capability.
        /// </summary>
        /// <param name="capability">Capability to check.</param>
        /// <returns>Capability Support Value for the provided Capability.</returns>
        public static CapabilitySupport GetCapabilitySupportValue(string capability)
        {
            switch (capability.ToLower())
            {
                case "buttonentity":
                case "canvasentity":
                case "characterentity":
                case "containerentity":
                case "htmlentity":
                case "inputentity":
                case "lightentity":
                case "meshentity":
                case "heightmapterrainentity":
                case "hybridterrainentity":
                case "textentity":
                case "voxelentity":
                    return CapabilitySupport.Supported;

                case "gltfloading":
                case "glbloading":
                    return CapabilitySupport.Supported;

                case "pngloading":
                    return CapabilitySupport.Supported;

                case "javascript":
                    return CapabilitySupport.Supported;

                case "localstorage":
                case "worldstorage":
                    return CapabilitySupport.Supported;

                case "http":
                case "mqtt":
                case "websocket":
                    return CapabilitySupport.Supported;

                case "vossynchronization":
                case "vss":
                    return CapabilitySupport.Supported;

                case "mediummemory":
                    if (SystemInfo.systemMemorySize > 8191)
                    {
                        return CapabilitySupport.Supported;
                    }
                    else
                    {
                        return CapabilitySupport.InsufficientResources;
                    }

                case "highmemory":
                    if (SystemInfo.systemMemorySize > 16383)
                    {
                        return CapabilitySupport.Supported;
                    }
                    else
                    {
                        return CapabilitySupport.InsufficientResources;
                    }

                case "veryhighmemory":
                    if (SystemInfo.systemMemorySize > 32767)
                    {
                        return CapabilitySupport.Supported;
                    }
                    else
                    {
                        return CapabilitySupport.InsufficientResources;
                    }

                case "mediumgpu":
                    if (SystemInfo.graphicsMemorySize > 4091)
                    {
                        return CapabilitySupport.Supported;
                    }
                    else
                    {
                        return CapabilitySupport.InsufficientResources;
                    }

                case "highgpu":
                    if (SystemInfo.graphicsMemorySize > 8191)
                    {
                        return CapabilitySupport.Supported;
                    }
                    else
                    {
                        return CapabilitySupport.InsufficientResources;
                    }

                case "veryhighgpu":
                    if (SystemInfo.graphicsMemorySize > 16383)
                    {
                        return CapabilitySupport.Supported;
                    }
                    else
                    {
                        return CapabilitySupport.InsufficientResources;
                    }

                default:
                    return CapabilitySupport.Unsupported;
            }
        }
    }
}