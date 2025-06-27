using System;
using System.Collections.Generic;
using UnityEngine;

namespace NWH.VehiclePhysics2.Modules.AirSteer
{
    /// <summary>
    ///     Module that adds steering to the vehicle while in the air.
    /// </summary>
    [Serializable]
    public partial class AirSteerModule : VehicleModule
    {
        /// <summary>
        /// Torque applied around the Y axis to steer the vehicle while in the air (nose left/right).
        /// Activated with steering input.
        /// </summary>
        [UnityEngine.Tooltip("Torque applied around the Y axis to steer the vehicle while in the air (nose left/right).\r\nActivated with steering input.")]
        public float yawTorque = 10000f;

        /// <summary>
        /// Torque applied around the X axis to steer the vehicle while in the air (nose up, down).
        /// Activated with throttle / brake input.
        /// Torque from the changes in the wheel angular velocity will get applied independently of this setting
        /// by the WheelController.
        /// </summary>
        [UnityEngine.Tooltip("Torque applied around the X axis to steer the vehicle while in the air (nose up, down).\r\nActivated with throttle / brake input.\r\nTorque from the changes in the wheel angular velocity will get applied independently of this setting\r\nby the WheelController.")]
        public float pitchTorque = 10000f;


        public override void Initialize()
        {
            base.Initialize();
        }


        public override void Update()
        {
  
        }


        public override void FixedUpdate()
        {
            if (!Active)
            {
                return;
            }

            if (vc.IsGrounded())
            {
                return;
            }

            Vector3 torque = Vector3.zero;
            torque.x = (vc.input.Throttle - vc.input.Brakes) * pitchTorque;
            torque.y = vc.input.Steering * yawTorque;

            vc.vehicleRigidbody.AddRelativeTorque(torque);
        }


        public override ModuleCategory GetModuleCategory()
        {
            return ModuleCategory.Control;
        }
    }
}