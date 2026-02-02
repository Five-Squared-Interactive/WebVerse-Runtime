// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.Audio;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for KHR_audio_emitter.
    /// Creates Unity AudioSource components for spatial audio.
    /// </summary>
    public class StraightFourAudioEmitterHandler : StraightFourNodeHandlerBase<KHRAudioEmitterNode>
    {
        public override string ExtensionName => "KHR_audio_emitter";
        public override int Priority => 50;

        public override Task OnNodeImportAsync(KHRAudioEmitterNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null || data.emitter < 0)
            {
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing audio emitter for node {nodeIndex}");

            // Get emitters from context
            if (!context.CustomData.TryGetValue("SF_AudioEmitters", out var emittersObj))
            {
                Logging.LogWarning("[StraightFour] No audio emitters found in context");
                return Task.CompletedTask;
            }

            var emitters = emittersObj as List<KHRAudioEmitter>;
            if (emitters == null || data.emitter >= emitters.Count)
            {
                Logging.LogWarning($"[StraightFour] Invalid emitter index {data.emitter}");
                return Task.CompletedTask;
            }

            var emitter = emitters[data.emitter];

            // Add audio emitter behavior
            var emitterComponent = targetObject.AddComponent<OMIAudioEmitterBehavior>();
            emitterComponent.Initialize(emitter, context);

            // Find parent entity using glTF parent node index
            BaseEntity parentEntity = null;
            if (context.CustomData != null && context.CustomData.TryGetValue("SF_NodeParentIndices", out var parentMapObj))
            {
                var parentMap = parentMapObj as Dictionary<int, int>;
                if (parentMap != null && parentMap.TryGetValue(nodeIndex, out var parentNodeIndex))
                {
                    parentEntity = GetEntityForNode(context, parentNodeIndex);
                }
            }
            // Create entity for the audio source with correct parent
            BaseEntity entity = GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);

            // Configure audio entity using adapter pattern (Task 2R.7)
            if (entity != null)
            {
                ConfigureAudioFromOMI(entity, data.emitter, context);
            }

            Logging.Log($"[StraightFour] Created audio emitter: {emitter.name ?? "unnamed"}, type={emitter.type}");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Configures audio entity by setting properties directly from OMI data.
        /// No configuration structs or methods - uses entity APIs directly.
        /// </summary>
        private void ConfigureAudioFromOMI(BaseEntity entity, int emitterIndex, OMIImportContext context)
        {
            if (!(entity is AudioEntity audioEntity))
            {
                Logging.LogWarning($"[StraightFourAudioEmitterHandler] Entity is not an AudioEntity: {entity.GetType().Name}");
                return;
            }

            // Get the raw glTF JSON to extract KHR_audio_emitter extension data
            if (!context.CustomData.TryGetValue("SF_GltfJson", out var jsonObj))
            {
                Logging.LogWarning("[StraightFourAudioEmitterHandler] No glTF JSON found in context");
                return;
            }

            var root = jsonObj as JObject;
            if (root == null)
            {
                return;
            }

            // Get document-level emitter definitions
            var extensions = root["extensions"] as JObject;
            var audioExt = extensions?["KHR_audio"] as JObject;
            var emitters = audioExt?["emitters"] as JArray;

            if (emitters == null || emitterIndex < 0 || emitterIndex >= emitters.Count)
            {
                Logging.LogWarning($"[StraightFourAudioEmitterHandler] Invalid emitter index {emitterIndex}");
                return;
            }

            var khrAudioEmitter = emitters[emitterIndex] as JObject;
            if (khrAudioEmitter == null)
            {
                return;
            }

            // Set properties directly on the audio entity
            // KHR field: gain → AudioEntity property: volume
            audioEntity.volume = khrAudioEmitter["gain"]?.Value<float>() ?? 1.0f;

            // Pitch property
            audioEntity.pitch = khrAudioEmitter["pitch"]?.Value<float>() ?? 1.0f;

            // Loop property
            audioEntity.loop = khrAudioEmitter["loop"]?.Value<bool>() ?? false;

            // KHR field: refDistance → AudioEntity property: minDistance
            audioEntity.minDistance = khrAudioEmitter["refDistance"]?.Value<float>() ?? 1.0f;

            // Max distance for attenuation
            audioEntity.maxDistance = khrAudioEmitter["maxDistance"]?.Value<float>() ?? 500f;

            // Fully 3D spatial audio
            audioEntity.spatialBlend = 1.0f;

            // Enable doppler effect
            audioEntity.dopplerLevel = khrAudioEmitter["dopplerLevel"]?.Value<float>() ?? 1.0f;

            // Rolloff factor determines rolloff mode
            float rolloffFactor = khrAudioEmitter["rolloffFactor"]?.Value<float>() ?? 1.0f;
            if (rolloffFactor <= 0.01f)
            {
                audioEntity.rolloffMode = AudioRolloffMode.Custom; // No attenuation
            }
            else if (rolloffFactor > 1.5f)
            {
                audioEntity.rolloffMode = AudioRolloffMode.Linear; // Fast falloff
            }
            else
            {
                audioEntity.rolloffMode = AudioRolloffMode.Logarithmic; // Realistic falloff
            }

            Logging.Log($"[StraightFourAudioEmitterHandler] Configured AudioEntity: volume={audioEntity.volume}, " +
                       $"loop={audioEntity.loop}, minDistance={audioEntity.minDistance}, maxDistance={audioEntity.maxDistance}");
        }
    }

    /// <summary>
    /// Behavior component for OMI audio emitters.
    /// </summary>
    public class OMIAudioEmitterBehavior : MonoBehaviour
    {
        public KHRAudioEmitter EmitterData { get; private set; }
        public AudioSource AudioSource { get; private set; }
        public bool IsPositional => EmitterData?.TypeEnum == AudioEmitterType.Positional;

        private List<KHRAudioSource> sources;
        private List<KHRAudioData> audioData;

        public void Initialize(KHRAudioEmitter emitter, OMIImportContext context)
        {
            EmitterData = emitter;

            // Get sources and audio data from context
            if (context.CustomData.TryGetValue(StraightFourCustomDataKeys.AudioSources, out var sourcesObj))
            {
                sources = sourcesObj as List<KHRAudioSource>;
            }

            if (context.CustomData.TryGetValue(StraightFourCustomDataKeys.AudioData, out var audioObj))
            {
                audioData = audioObj as List<KHRAudioData>;
            }

            // Create AudioSource component
            AudioSource = gameObject.GetComponent<AudioSource>();
            if (AudioSource == null)
            {
                AudioSource = gameObject.AddComponent<AudioSource>();
            }

            // Configure based on emitter type
            ConfigureAudioSource();
        }

        private void ConfigureAudioSource()
        {
            if (EmitterData == null || AudioSource == null)
                return;

            // Set gain
            AudioSource.volume = EmitterData.gain;

            // Configure spatial blend based on type
            if (EmitterData.TypeEnum == AudioEmitterType.Global)
            {
                AudioSource.spatialBlend = 0f; // 2D audio
            }
            else
            {
                AudioSource.spatialBlend = 1f; // Full 3D audio
                ConfigurePositionalAudio();
            }

            // Configure from first source
            if (EmitterData.sources != null && EmitterData.sources.Length > 0 && sources != null)
            {
                int sourceIndex = EmitterData.sources[0];
                if (sourceIndex >= 0 && sourceIndex < sources.Count)
                {
                    var source = sources[sourceIndex];
                    AudioSource.loop = source.loop;
                    AudioSource.pitch = source.playbackRate;
                    AudioSource.volume *= source.gain;

                    if (source.autoplay)
                    {
                        // Will need to load audio clip first
                        // For now, mark for autoplay
                        AudioSource.playOnAwake = true;
                    }

                    // TODO: Load actual audio clip from source.audio or extensions
                    // This would require async audio loading
                }
            }
        }

        private void ConfigurePositionalAudio()
        {
            var positional = EmitterData.positional;
            if (positional == null)
                return;

            // Configure distance attenuation
            switch (positional.DistanceModelEnum)
            {
                case AudioDistanceModel.Linear:
                    AudioSource.rolloffMode = AudioRolloffMode.Linear;
                    break;
                case AudioDistanceModel.Exponential:
                    AudioSource.rolloffMode = AudioRolloffMode.Custom;
                    // Would need to set up custom curve
                    break;
                default:
                    AudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                    break;
            }

            AudioSource.minDistance = positional.refDistance;
            if (positional.maxDistance > 0)
            {
                AudioSource.maxDistance = positional.maxDistance;
            }

            // Configure cone for directional audio
            if (positional.ShapeTypeEnum == AudioShapeType.Cone)
            {
                // Unity doesn't have native cone support, would need custom solution
                // Store values for potential custom implementation
            }
        }

        /// <summary>
        /// Play the audio.
        /// </summary>
        public void Play()
        {
            if (AudioSource != null && AudioSource.clip != null)
            {
                AudioSource.Play();
            }
        }

        /// <summary>
        /// Stop the audio.
        /// </summary>
        public void Stop()
        {
            if (AudioSource != null)
            {
                AudioSource.Stop();
            }
        }

        /// <summary>
        /// Pause the audio.
        /// </summary>
        public void Pause()
        {
            if (AudioSource != null)
            {
                AudioSource.Pause();
            }
        }

        /// <summary>
        /// Set the audio clip to play.
        /// </summary>
        public void SetClip(AudioClip clip)
        {
            if (AudioSource != null)
            {
                AudioSource.clip = clip;
            }
        }
    }
}
#endif
