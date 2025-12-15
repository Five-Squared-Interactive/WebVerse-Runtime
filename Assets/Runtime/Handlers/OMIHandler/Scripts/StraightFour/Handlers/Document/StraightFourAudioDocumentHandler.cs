// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.Audio;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Document-level handler for KHR_audio_emitter.
    /// Stores audio data, sources, and emitter definitions.
    /// </summary>
    public class StraightFourAudioDocumentHandler : StraightFourDocumentHandlerBase<KHRAudioEmitterRoot>
    {
        public override string ExtensionName => "KHR_audio_emitter";
        public override int Priority => 94;

        public override Task OnDocumentImportAsync(KHRAudioEmitterRoot data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null)
            {
                return Task.CompletedTask;
            }

            // Store audio data (raw audio files/buffers)
            if (data.audio != null && data.audio.Length > 0)
            {
                var audioData = new List<KHRAudioData>(data.audio);
                context.CustomData[StraightFourCustomDataKeys.AudioData] = audioData;
                LogVerbose(context, $"[StraightFour] Stored {audioData.Count} audio data entries from document.");
            }

            // Store audio sources (playback settings)
            if (data.sources != null && data.sources.Length > 0)
            {
                var sources = new List<KHRAudioSource>(data.sources);
                context.CustomData[StraightFourCustomDataKeys.AudioSources] = sources;
                LogVerbose(context, $"[StraightFour] Stored {sources.Count} audio sources from document.");
            }

            // Store emitters (for node handlers to reference)
            if (data.emitters != null && data.emitters.Length > 0)
            {
                context.CustomData["SF_AudioEmitters"] = new List<KHRAudioEmitter>(data.emitters);
                LogVerbose(context, $"[StraightFour] Stored {data.emitters.Length} audio emitters from document.");
            }

            // TODO: Actually load audio clips from URIs or buffer views
            // This would require async loading of audio files
            // For now, we just store the metadata

            return Task.CompletedTask;
        }
    }
}
#endif
