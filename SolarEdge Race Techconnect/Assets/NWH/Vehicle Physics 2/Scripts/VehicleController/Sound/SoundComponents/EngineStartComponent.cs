using System;
using UnityEngine;

namespace NWH.VehiclePhysics2.Sound.SoundComponents
{
    /// <summary>
    ///     Sound of an engine starting / stopping.
    ///     Plays while start is active.
    ///     Clip at index 0 should be an engine starting sound, clip at 1 should be an engine stopping sound (optional).
    /// </summary>
    [Serializable]
    public partial class EngineStartComponent : SoundComponent
    {
        private bool _isPlayingStarting = false;
        
        public override void Initialize()
        {
            base.Initialize();
            
            vc.powertrain.engine.OnStart.AddListener(() => { _isPlayingStarting = true;});
            vc.powertrain.engine.OnStop.AddListener(PlayStopping);
        }

        public override void Update()
        {
            if (!Active)
            {
                return;
            }

            if (_isPlayingStarting)
            {
                PlayStarting();
            }
        }

        public virtual void PlayStarting()
        {
            // Starting and stopping engine sound
            if (Source != null && Clips.Count > 0)
            {
                if (vc.powertrain.engine.StarterActive)
                {
                    if (!Source.isPlaying)
                    {
                        Source.loop = true;
                        SetVolume(baseVolume);
                        Play(0, 0);
                    }
                }
                else
                {
                    _isPlayingStarting = false;
                    
                    if (Source.isPlaying)
                    {
                        Stop();
                    }
                }
            }
        }

        public virtual void PlayStopping()
        {
            _isPlayingStarting = false;
            if (Source != null && Clips.Count > 1)
            {
                Source.loop = false;
                SetVolume(baseVolume);
                Play(0, 1);
            }
        }


        public override void FixedUpdate()
        {
        }


        public override void SetDefaults(VehicleController vc)
        {
            base.SetDefaults(vc);
            baseVolume = 0.2f;
            basePitch  = 1f;

            if (Clip == null)
            {
                AddDefaultClip("EngineStart");               
            }
        }
    }
}