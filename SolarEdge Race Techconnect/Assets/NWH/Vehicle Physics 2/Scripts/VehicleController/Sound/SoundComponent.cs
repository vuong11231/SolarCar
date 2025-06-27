using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace NWH.VehiclePhysics2.Sound.SoundComponents
{
    /// <summary>
    ///     Base class for all vehicle SoundComponents.
    ///     Inserts a layer above Unity's AudioSource(s) which insures that the values are set properly, master volume is used,
    ///     etc.
    ///     Supports multiple AudioSources/AudioClips per one SoundComponent for random clip switching.
    /// </summary>
    [Serializable]
    public abstract class SoundComponent : VehicleComponent
    {
        /// <summary>
        ///     AudioMixerGroup that this SoundComponent belongs to.
        /// </summary>
        [Tooltip("    AudioMixerGroup that this SoundComponent belongs to.")]
        public AudioMixerGroup audioMixerGroup;

        /// <summary>
        ///     Base pitch of the sound component.
        /// </summary>
        [FormerlySerializedAs("pitch")]
        [Range(0f, 2f)]
        [Tooltip("    Base pitch of the sound component.")]
        public float basePitch = 1f;

        /// <summary>
        ///     Base volume of the sound component.
        /// </summary>
        [FormerlySerializedAs("volume")]
        [Range(0f, 1f)]
        [Tooltip("    Base volume of the sound component.")]
        public float baseVolume = 0.1f;

        /// <summary>
        ///     List of audio clips this component can use. Some components can use multiple clips in which case they will be
        ///     chosen at random, and some components can use only one
        ///     in which case only the first clip will be selected. Check manual for more details.
        /// </summary>
        [Tooltip(
            "List of audio clips this component can use. Some components can use multiple clips in which case they will be chosen at random, and some components can use only one " +
            "in which case only the first clip will be selected. Check manual for more details.")]
        public List<AudioClip> clips = new List<AudioClip>();
        
        public List<AudioSource> sources = new List<AudioSource>();

        protected GameObject sourceContainer;
        
        /// <summary>
        ///     Gets or sets the first clip in the clips list.
        /// </summary>
        public AudioClip Clip
        {
            get { return clips.Count > 0 ? clips[0] : null; }
            set
            {
                if (clips.Count > 0)
                {
                    clips[0] = value;
                }
                else
                {
                    clips.Add(value);
                }
            }
        }

        public void InitializeSoundComponent(AudioMixerGroup mixerGroup, GameObject sourceContainer)
        {
            #if NVP2_DEBUG
            Debug.Log($"-- InitializeSoundComponent() [{GetType().Name}]");
            #endif
            
            this.audioMixerGroup = mixerGroup;
            this.sourceContainer = sourceContainer;

            if (GetInitAutoSetupSource())
            {
                SetupAndRegisterAudioSource(AddSourceToContainer());
            }

            // Call VehicleComponent Initialize
            Initialize();
        }


        protected AudioSource AddSourceToContainer()
        {
            return sourceContainer.AddComponent<AudioSource>();
        }

        protected void SetupAndRegisterAudioSource(AudioSource source)
        {
            if (source == null)
            {
                Debug.LogError("Trying to setup a null AudioSource.");
                return;
            }
            
            source.outputAudioMixerGroup = audioMixerGroup;
            source.spatialBlend = GetInitSpatialBlend();
            source.playOnAwake  = GetInitPlayOnAwake();
            source.loop         = GetInitLoop();
            source.volume       = GetInitVolume() * (vc == null ? 1f : vc.soundManager.masterVolume);
            source.clip         = GetInitClip();
            source.priority     = 200;
            source.dopplerLevel = vc.soundManager.dopplerLevel;

            int index = sources.Count;
            sources.Add(source);

            if (GetInitPlayOnAwake())
            {
                Play(index);
            }
            else
            {
                Stop(index);
            }
        }

        /// <summary>
        /// If set to true a single audio source will be set up 
        /// </summary>
        /// <returns></returns>
        public virtual bool GetInitAutoSetupSource()
        {
            return true;
        }

        /// <summary>
        /// Override to set the initial source loop value.
        /// </summary>
        public virtual bool GetInitLoop()
        {
            return false;
        }

        /// <summary>
        /// Override to set the initial AudioClip value.
        /// </summary>
        public virtual AudioClip GetInitClip()
        {
            return Clip;
        }

        /// <summary>
        /// Override to set the initial source volume.
        /// </summary>
        public virtual float GetInitVolume()
        {
            return baseVolume;
        }

        /// <summary>
        /// Override to set the initial spatial blend.
        /// </summary>
        public virtual float GetInitSpatialBlend()
        {
            return vc.soundManager.spatialBlend;
        }
        
        /// <summary>
        /// Override to set the initial source play on awake setting.
        /// </summary>
        public virtual bool GetInitPlayOnAwake()
        {
            return false;
        }

        /// <summary>
        ///     Gets or sets the whole clip list.
        /// </summary>
        public List<AudioClip> Clips
        {
            get { return clips; }
            set { clips = value; }
        }

        /// <summary>
        ///     Gets a random clip from clips list.
        /// </summary>
        public AudioClip RandomClip
        {
            get { return clips[Random.Range(0, clips.Count)]; }
        }

        /// <summary>
        ///     Gets or sets the first audio source in the sources list.
        /// </summary>
        public AudioSource Source
        {
            get
            {
                if (sources.Count > 0)
                {
                    return sources[0];
                }

                return null;
            }
            set
            {
                if (sources.Count > 0)
                {
                    sources[0] = value;
                }

                sources.Add(value);
            }
        }

        /// <summary>
        ///     AudioSources belonging to this SoundComponent.
        /// </summary>
        public List<AudioSource> Sources
        {
            get { return sources; }
            set { sources = value; }
        }


        /// <summary>
        ///     Enables all the AudioSources belonging to this SoundComponent.
        ///     Calls Play() on all the looping sources.
        /// </summary>
        public override void Enable()
        {
            base.Enable();
            
            foreach (AudioSource source in sources)
            {
                if (!source.enabled)
                {
                    source.enabled = true;
                    if (source.loop)
                    {
                        source.Play();
                    }
                }
            }
        }


        /// <summary>
        ///     Disables all the AudioSources belonging to this SoundComponent.
        ///     Will call Stop() as well as disable the source.
        /// </summary>
        public override void Disable()
        {
            base.Disable();
            
            foreach (AudioSource source in sources)
            {
                if (source.isPlaying)
                {
                    Stop();
                }

                if (source.enabled)
                {
                    source.enabled = false;
                }

                source.volume = 0;
            }
        }


        /// <summary>
        ///     Gets pitch of the Source. Equal to Source.pitch.
        /// </summary>
        public virtual float GetPitch()
        {
            if (!Active)
            {
                return 0;
            }

            return Source.pitch;
        }


        /// <summary>
        ///     Gets volume of the Source. Equal to Source.volume.
        /// </summary>
        public virtual float GetVolume()
        {
            if (!Active)
            {
                return 0;
            }

            return Source.volume;
        }

        /// <summary>
        ///     Plays the source at index if not already playing.
        /// </summary>
        /// <param name="sourceIndex">Index of the source to play.</param>
        public virtual void Play(int sourceIndex = 0, int clipIndex = 0)
        {
            if (!Active)
            {
                return;
            }

            AudioSource s = Sources[sourceIndex];
#if NVP2_DEBUG
            Debug.Log($"{vc.name}: Play {GetType().Name}");
#endif
            
            if (s.isPlaying)
            {
                // Debug.LogWarning($"{vc.name}: {GetType().Name} is already playing.");
                return;
            }

            if (Clips.Count > clipIndex)
            {
                s.clip = Clips[clipIndex];
            }
            else
            {
                return; // Sometimes no clip is normal as e.g. surface has not yet been set.
                // Debug.LogWarning($"{GetType().Name}: Trying to play clip at index that does not exist. Ignored." +
                //                  $"Source: {sourceIndex}, Clip: {clipIndex}, Sources: {sources.Count}, Clips: {clips.Count}.");
            }

            s.Play();
        }


        /// <summary>
        ///     Sets pitch for the [id]th source in sources list.
        /// </summary>
        /// <param name="pitch">Pitch to set.</param>
        /// <param name="index">Index of the source.</param>
        public virtual void SetPitch(float pitch, int index)
        {
            if (!Active)
            {
                return;
            }

            pitch                = pitch < 0 ? 0 : pitch > 5 ? 5 : pitch;
            sources[index].pitch = pitch;
        }


        /// <summary>
        ///     Sets pitch for the first source in sources list.
        /// </summary>
        /// <param name="pitch">Pitch to set.</param>
        public virtual void SetPitch(float pitch)
        {
            if (!Active)
            {
                return;
            }

            pitch        = pitch < 0 ? 0 : pitch > 5 ? 5 : pitch;
            Source.pitch = pitch;
        }


        /// <summary>
        ///     Sets volume for the [id]th source in sources list. Use instead of directly changing source volume as this takes
        ///     master volume into account.
        /// </summary>
        /// <param name="volume">Volume to set.</param>
        /// <param name="index">Index of the target source.</param>
        public virtual void SetVolume(float volume, int index)
        {
            if (!Active)
            {
                return;
            }

            volume                = volume < 0 ? 0 : volume > 2 ? 2 : volume;
            sources[index].volume = volume * vc.soundManager.masterVolume;
        }


        /// <summary>
        ///     Sets the volume of AudioSource. Takes master volume into account.
        /// </summary>
        /// <param name="volume">Volume to set.</param>
        /// <param name="source">Target AudioSource.</param>
        public virtual void SetVolume(float volume, AudioSource source)
        {
            if (!Active)
            {
                return;
            }

            volume        = volume < 0 ? 0 : volume > 2 ? 2 : volume;
            source.volume = volume * vc.soundManager.masterVolume;
        }


        /// <summary>
        ///     Sets volume for the first source in sources list. Use instead of directly changing source volume as this takes
        ///     master volume into account.
        /// </summary>
        /// <param name="volume">Volume to set.</param>
        public virtual void SetVolume(float volume)
        {
            if (!Active)
            {
                return;
            }

            Source.volume = volume * vc.soundManager.masterVolume;
        }

        /// <summary>
        ///     Stops the AudioSource at index if already playing.
        /// </summary>
        /// <param name="index">Target AudioSource index.</param>
        public virtual void Stop(int index = 0)
        {
            AudioSource s = Sources[index];
#if NVP2_DEBUG
            Debug.Log($"{vc.name}: Stop {GetType().Name}");
#endif
            
            if (!s.isPlaying)
            {
                return;
            }

            s.Stop();
        }


        public virtual void AddDefaultClip(string clipName)
        {
            Clip = Resources.Load(VehicleController.defaultResourcesPath + "Sound/" + clipName) as AudioClip;
            if (Clip == null)
            {
                Debug.LogWarning(
                    $"Audio Clip for sound component {GetType().Name} could not be loaded from resources. " +
                    $"Source will not play." +
                    $"Assign an AudioClip manually.");
            }
        }
        
        
        /// <summary>
        /// Gets the SoundComponent network state. Minimum required info to transfer the sound settings over the network.
        /// </summary>
        public virtual void GetNetworkState(out bool isPlaying, out float volume, out float pitch)
        {
            if (Source == null)
            {
                isPlaying = false;
                volume = 0;
                pitch = 0;
                return;
            }
            
            isPlaying = Source.isPlaying;
            volume = Source.volume;
            pitch = Source.pitch;
        }
        

        /// <summary>
        /// Sets the state to network values. To be used together with GetNetworkState().
        /// </summary>
        public virtual void SetNetworkState(bool isPlaying, float volume, float pitch)
        {
            if (isPlaying && !Source.isPlaying)
            {
                Play();
            }
            else if (!isPlaying && Source.isPlaying)
            {
                Stop();
            }

            if (isPlaying)
            {
                // Do not use SetVolume and SetPitch as that will apply masterVolume which is already applied in this case.
                Source.volume = volume;
                Source.pitch = pitch;
            }
        }
    }
}