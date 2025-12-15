// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.PhysicsBody;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Document-level handler for OMI_physics_body.
    /// Stores physics materials and collision filters that can be referenced by physics bodies.
    /// </summary>
    public class StraightFourPhysicsBodyDocumentHandler : StraightFourDocumentHandlerBase<OMIPhysicsBodyRoot>
    {
        public override string ExtensionName => OMIPhysicsBodyExtension.ExtensionName;
        public override int Priority => 99; // After shapes

        public override Task OnDocumentImportAsync(OMIPhysicsBodyRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null)
            {
                return Task.CompletedTask;
            }

            // Store physics materials
            if (data.PhysicsMaterials != null && data.PhysicsMaterials.Length > 0)
            {
                var materials = new List<OMIPhysicsMaterial>(data.PhysicsMaterials);
                context.CustomData[StraightFourCustomDataKeys.PhysicsMaterials] = materials;
                LogVerbose(context, $"[StraightFour] Stored {materials.Count} physics materials from document.");
            }

            // Store collision filters
            if (data.CollisionFilters != null && data.CollisionFilters.Length > 0)
            {
                var filters = new List<OMICollisionFilter>(data.CollisionFilters);
                context.CustomData[StraightFourCustomDataKeys.CollisionFilters] = filters;
                LogVerbose(context, $"[StraightFour] Stored {filters.Count} collision filters from document.");
            }

            return Task.CompletedTask;
        }
    }
}
#endif
