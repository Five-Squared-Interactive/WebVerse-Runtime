// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.EnvironmentSky;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Document-level handler for OMI_environment_sky.
    /// Stores sky definitions that can be referenced by scenes.
    /// </summary>
    public class StraightFourEnvironmentSkyDocumentHandler : StraightFourDocumentHandlerBase<OMIEnvironmentSkyDocumentData>
    {
        public override string ExtensionName => "OMI_environment_sky";
        public override int Priority => 95;

        public override Task OnDocumentImportAsync(OMIEnvironmentSkyDocumentData data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data?.Skies == null || data.Skies.Count == 0)
            {
                return Task.CompletedTask;
            }

            // Store sky definitions for scene handler to reference
            context.CustomData[StraightFourCustomDataKeys.Skies] = data.Skies;

            LogVerbose(context, $"[StraightFour] Stored {data.Skies.Count} sky definitions from document.");

            // Log sky details if verbose
            if (context.Settings.VerboseLogging)
            {
                for (int i = 0; i < data.Skies.Count; i++)
                {
                    var sky = data.Skies[i];
                    Logging.Log($"[StraightFour]   Sky {i}: type={sky.Type ?? "unknown"}");
                }
            }

            return Task.CompletedTask;
        }
    }
}
#endif
