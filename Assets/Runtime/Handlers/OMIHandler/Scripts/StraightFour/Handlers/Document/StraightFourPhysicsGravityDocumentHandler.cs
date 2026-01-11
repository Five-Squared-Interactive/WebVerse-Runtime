// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.PhysicsGravity;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Document-level handler for OMI_physics_gravity.
    /// Sets the world gravity based on document-level extension data.
    /// </summary>
    public class StraightFourPhysicsGravityDocumentHandler : StraightFourDocumentHandlerBase<OMIPhysicsGravityRoot>
    {
        public override string ExtensionName => OMIPhysicsGravityExtension.ExtensionName;
        public override int Priority => 98;

        public override Task OnDocumentImportAsync(OMIPhysicsGravityRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null)
            {
                return Task.CompletedTask;
            }

            // Store for reference
            context.CustomData[StraightFourCustomDataKeys.WorldGravity] = data;

            // Apply world gravity
            Vector3 direction = ToVector3(data.Direction, new Vector3(0, -1, 0));
            Vector3 gravity = direction.normalized * data.Gravity;
            
            Physics.gravity = gravity;
            
            Logging.Log($"[StraightFour] Set world gravity to {gravity} (magnitude: {data.Gravity} m/s²)");

            return Task.CompletedTask;
        }
    }
}
#endif
