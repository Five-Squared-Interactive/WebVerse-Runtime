// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.PhysicsJoint;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Document-level handler for OMI_physics_joint.
    /// Stores joint settings definitions that can be referenced by node-level joints.
    /// </summary>
    public class StraightFourPhysicsJointDocumentHandler : StraightFourDocumentHandlerBase<OMIPhysicsJointRoot>
    {
        public override string ExtensionName => OMIPhysicsJointExtension.ExtensionName;
        public override int Priority => 97;

        public override Task OnDocumentImportAsync(OMIPhysicsJointRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data?.PhysicsJoints == null || data.PhysicsJoints.Length == 0)
            {
                return Task.CompletedTask;
            }

            // Store joint settings for node handlers to reference
            var joints = new List<OMIPhysicsJointSettings>(data.PhysicsJoints);
            context.CustomData[StraightFourCustomDataKeys.JointSettings] = joints;

            LogVerbose(context, $"[StraightFour] Stored {joints.Count} joint settings from document.");

            return Task.CompletedTask;
        }
    }
}
#endif
