using System;
using NWH.VehiclePhysics2.Effects;
using UnityEngine;

namespace NWH.VehiclePhysics2.Sound.SoundComponents
{
    /// <summary>
    ///     Click-clack of the working blinker.
    ///     Accepts two clips, first is for the blinker turning on and the second is for blinker turning off.
    /// </summary>
    [Serializable]
    public partial class BlinkerComponent : SoundComponent
    {
        public override void Initialize()
        {
            base.Initialize();
            
            if (vc.VehicleMultiplayerInstanceType == Vehicle.MultiplayerInstanceType.Local)
            {
                foreach (LightSource ls in vc.effectsManager.lightsManager.leftBlinkers.lightSources)
                {
                    ls.onLightTurnedOn.AddListener(PlayBlinkerOn);
                    ls.onLightTurnedOff.AddListener(PlayBlinkerOff);
                }

                foreach (LightSource ls in vc.effectsManager.lightsManager.rightBlinkers.lightSources)
                {
                    ls.onLightTurnedOn.AddListener(PlayBlinkerOn);
                    ls.onLightTurnedOff.AddListener(PlayBlinkerOff);
                }
            }
        }

        private void PlayBlinkerOn()
        {
            Source.volume = baseVolume;
            Source.pitch  = basePitch;
            
            if (Clips.Count == 2)
            {
                Play(0, 1);
            }
            else
            {
                Play(0, 0);
            }
        }

        private void PlayBlinkerOff()
        {
            Source.volume = baseVolume;
            Source.pitch  = basePitch;
            Play(0, 0);
        }

        public override void Update()
        {
        }

        public override void FixedUpdate()
        {
        }


        public override void SetDefaults(VehicleController vc)
        {
            base.SetDefaults(vc);

            baseVolume = 0.8f;
            basePitch  = 1f;

            if (Clip == null)
            {
                AddDefaultClip("BlinkerOn");
                AddDefaultClip("BlinkerOff");
            }
        }
    }
}