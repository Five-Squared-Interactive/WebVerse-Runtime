// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.PhysicsShape;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Document-level handler for OMI_physics_shape.
    /// Stores shape definitions that can be referenced by physics bodies.
    /// </summary>
    public class StraightFourPhysicsShapeHandler : StraightFourDocumentHandlerBase<OMIPhysicsShapeRoot>
    {
        public override string ExtensionName => OMIPhysicsShapeExtension.ExtensionName;
        public override int Priority => 100; // Process first so shapes are available for bodies

        public override Task OnDocumentImportAsync(OMIPhysicsShapeRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data?.Shapes == null || data.Shapes.Length == 0)
            {
                LogVerbose(context, "[StraightFour] No physics shapes in document.");
                return Task.CompletedTask;
            }

            // Store shapes in context for node handlers to reference
            var shapes = new List<OMIPhysicsShape>(data.Shapes);
            context.CustomData[StraightFourCustomDataKeys.PhysicsShapes] = shapes;

            LogVerbose(context, $"[StraightFour] Stored {shapes.Count} physics shapes from document.");

            // Log shape details if verbose
            if (context.Settings.VerboseLogging)
            {
                for (int i = 0; i < shapes.Count; i++)
                {
                    var shape = shapes[i];
                    Logging.Log($"[StraightFour]   Shape {i}: type={shape.Type}");
                }
            }

            return Task.CompletedTask;
        }
    }
}
#endif
