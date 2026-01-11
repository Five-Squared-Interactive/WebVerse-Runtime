// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.StraightFour.Utilities;
using System;
using UnityEngine;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for an audio entity.
    /// </summary>
    public class AudioEntity : BaseEntity
    {
        /// <summary>
        /// Audio clip to play.
        /// </summary>
        public AudioClip audioClip
        {
            get
            {
                return audioSourceObject.clip;
            }
            set
            {
                audioSourceObject.clip = value;
            }
        }

        /// <summary>
        /// Whether or not to loop the audio clip.
        /// </summary>
        public bool loop
        {
            get
            {
                return audioSourceObject.loop;
            }
            set
            {
                audioSourceObject.loop = value;
            }
        }

        /// <summary>
        /// Priority for the audio clip. Values between 0 and 256, with 0 being highest priority.
        /// </summary>
        public int priority
        {
            get
            {
                return audioSourceObject.priority;
            }
            set
            {
                audioSourceObject.priority = value;
            }
        }

        /// <summary>
        /// Volume for the audio clip. Values between 0 and 1, with 1 being highest volume.
        /// </summary>
        public float volume
        {
            get
            {
                return audioSourceObject.volume;
            }
            set
            {
                audioSourceObject.volume = value;
            }
        }

        /// <summary>
        /// Pitch for the audio clip. Values between -3 and 3.
        /// </summary>
        public float pitch
        {
            get
            {
                return audioSourceObject.pitch;
            }
            set
            {
                audioSourceObject.pitch = value;
            }
        }

        /// <summary>
        /// Audio pan for the audio clip if playing in stereo. Values between -1 and 1, with -1
        /// being furthest to the left and 1 being furthest to the right.
        /// </summary>
        public float stereoPan
        {
            get
            {
                return audioSourceObject.panStereo;
            }
            set
            {
                audioSourceObject.panStereo = value;
            }
        }

        /// <summary>
        /// The audio source object.
        /// </summary>
        private AudioSource audioSourceObject;

        /// <summary>
        /// Play the audio.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Play()
        {
            audioSourceObject.Play();

            return true;
        }

        /// <summary>
        /// Stop playing the audio.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Stop()
        {
            audioSourceObject.Stop();

            return true;
        }

        /// <summary>
        /// Toggle pausing the audio.
        /// </summary>
        /// <param name="pause">Whether or not to pause.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool TogglePause(bool pause)
        {
            if (pause)
            {
                audioSourceObject.Pause();
            }
            else
            {
                audioSourceObject.UnPause();
            }

            return true;
        }

        /// <summary>
        /// Set the interaction state for the entity.
        /// </summary>
        /// <param name="stateToSet">Interaction state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetInteractionState(InteractionState stateToSet)
        {
            switch (stateToSet)
            {
                case InteractionState.Physical:
                    LogSystem.LogWarning("[AudioEntity->SetInteractionState] Physical not valid for audio.");
                    return true;

                case InteractionState.Placing:
                    LogSystem.LogWarning("[AudioEntity->SetInteractionState] Placing not valid for audio.");
                    return false;

                case InteractionState.Static:
                    MakeStatic();
                    return true;

                case InteractionState.Hidden:
                    MakeHidden();
                    return true;

                default:
                    LogSystem.LogWarning("[AudioEntity->SetInteractionState] Interaction state invalid.");
                    return false;
            }
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(Guid idToSet)
        {
            base.Initialize(idToSet);

            audioSourceObject = gameObject.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Tear down the entity.
        /// </summary>
        public override void TearDown()
        {
            base.TearDown();
        }

        /// <summary>
        /// Make the entity hidden.
        /// </summary>
        private void MakeHidden()
        {
            switch (interactionState)
            {
                case InteractionState.Physical:
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                    break;

                case InteractionState.Hidden:
                default:
                    break;
            }

            gameObject.SetActive(false);
            interactionState = InteractionState.Hidden;
        }

        /// <summary>
        /// Make the entity static.
        /// </summary>
        private void MakeStatic()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    // Handled in main sequence.
                    break;

                case InteractionState.Physical:
                    // Handled in main sequence.
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                default:
                    break;
            }

            gameObject.SetActive(true);
            interactionState = InteractionState.Static;
        }
    }
}