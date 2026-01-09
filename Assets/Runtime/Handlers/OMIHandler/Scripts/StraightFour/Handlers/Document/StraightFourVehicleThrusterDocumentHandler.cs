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
    /// Document-level handler for OMI_vehicle_thruster.
    /// Stores thruster settings definitions that can be referenced by node-level thrusters.
    /// </summary>
    public class StraightFourVehicleThrusterDocumentHandler : StraightFourDocumentHandlerBase<OMIVehicleThrusterRoot>
    {
        public override string ExtensionName => "OMI_vehicle_thruster";
        public override int Priority => 92;

        public override Task OnDocumentImportAsync(OMIVehicleThrusterRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            Logging.Log($"[StraightFour] VehicleThrusterDocumentHandler called, data null={data == null}");
            
            if (data?.thrusters == null || data.thrusters.Length == 0)
            {
                Logging.Log($"[StraightFour] No thrusters in document extension");
                return Task.CompletedTask;
            }

            // Store thruster settings for node handlers to reference
            var thrusters = new List<OMIVehicleThrusterSettings>(data.thrusters);
            context.CustomData["SF_VehicleThrusters"] = thrusters;

            Logging.Log($"[StraightFour] Stored {thrusters.Count} thruster settings from document.");

            // Log thruster details if verbose
            if (context.Settings.VerboseLogging)
            {
                for (int i = 0; i < thrusters.Count; i++)
                {
                    var thruster = thrusters[i];
                    Logging.Log($"[StraightFour]   Thruster {i}: maxForce={thruster.maxForce}N, maxGimbal={thruster.maxGimbal}rad");
                }
            }

            return Task.CompletedTask;
        }
    }
}
#endif
