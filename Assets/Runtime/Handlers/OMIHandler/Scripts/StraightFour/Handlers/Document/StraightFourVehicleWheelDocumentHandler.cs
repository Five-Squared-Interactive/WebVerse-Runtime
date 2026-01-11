// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.Vehicle;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Document-level handler for OMI_vehicle_wheel.
    /// Stores wheel settings definitions that can be referenced by node-level wheels.
    /// </summary>
    public class StraightFourVehicleWheelDocumentHandler : StraightFourDocumentHandlerBase<OMIVehicleWheelRoot>
    {
        public override string ExtensionName => "OMI_vehicle_wheel";
        public override int Priority => 93;

        public override Task OnDocumentImportAsync(OMIVehicleWheelRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data?.wheels == null || data.wheels.Length == 0)
            {
                return Task.CompletedTask;
            }

            // Store wheel settings for node handlers to reference
            var wheels = new List<OMIVehicleWheelSettings>(data.wheels);
            context.CustomData[StraightFourCustomDataKeys.WheelSettings] = wheels;

            LogVerbose(context, $"[StraightFour] Stored {wheels.Count} wheel settings from document.");

            // Log wheel details if verbose
            if (context.Settings.VerboseLogging)
            {
                for (int i = 0; i < wheels.Count; i++)
                {
                    var wheel = wheels[i];
                    Logging.Log($"[StraightFour]   Wheel {i}: radius={wheel.radius}m, maxForce={wheel.maxForce}N");
                }
            }

            return Task.CompletedTask;
        }
    }
}
#endif
