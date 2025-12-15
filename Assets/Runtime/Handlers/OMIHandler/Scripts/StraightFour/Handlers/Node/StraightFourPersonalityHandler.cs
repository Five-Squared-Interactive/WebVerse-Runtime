// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.Personality;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_personality.
    /// Creates NPC/agent behavior components with personality data.
    /// </summary>
    public class StraightFourPersonalityHandler : StraightFourNodeHandlerBase<OMIPersonalityNode>
    {
        public override string ExtensionName => OMIPersonalityExtension.ExtensionName;
        public override int Priority => 50;

        public override Task OnNodeImportAsync(OMIPersonalityNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null)
            {
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing personality for node {nodeIndex}: {data.Agent}");

            // Add personality component
            var personalityComponent = targetObject.AddComponent<OMIPersonalityBehavior>();
            personalityComponent.Initialize(data);

            // Create entity for the NPC
            GetOrCreateEntity(context, nodeIndex, targetObject, null);

            Logging.Log($"[StraightFour] Created personality for agent: {data.Agent}");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Behavior component for OMI personality/NPC data.
    /// Provides AI character personality information for integration with language models.
    /// </summary>
    public class OMIPersonalityBehavior : MonoBehaviour
    {
        /// <summary>
        /// The name of this agent/NPC.
        /// </summary>
        public string AgentName { get; private set; }

        /// <summary>
        /// The personality description for this agent.
        /// Can be used as context for language models.
        /// </summary>
        public string Personality { get; private set; }

        /// <summary>
        /// Optional default message to initialize conversation.
        /// </summary>
        public string DefaultMessage { get; private set; }

        public void Initialize(OMIPersonalityNode data)
        {
            AgentName = data.Agent;
            Personality = data.Personality;
            DefaultMessage = data.DefaultMessage;
        }

        /// <summary>
        /// Gets the system prompt for this agent's personality.
        /// Suitable for injection into language model context.
        /// </summary>
        public string GetSystemPrompt()
        {
            return $"You are {AgentName}. {Personality}";
        }

        /// <summary>
        /// Gets the initial greeting/message for this agent.
        /// </summary>
        public string GetInitialMessage()
        {
            return DefaultMessage ?? $"Hello, I am {AgentName}.";
        }
    }
}
#endif
