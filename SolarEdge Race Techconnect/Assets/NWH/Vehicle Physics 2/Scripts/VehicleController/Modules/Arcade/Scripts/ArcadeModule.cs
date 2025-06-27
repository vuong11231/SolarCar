using System;
using System.Collections.Generic;
using NWH.Common.Utility;
using NWH.VehiclePhysics2.Powertrain;
using NWH.VehiclePhysics2.Powertrain.Wheel;
using NWH.WheelController3D;
using UnityEngine;

namespace NWH.VehiclePhysics2.Modules.ArcadeModule
{
    /// <summary>
    ///     Arcade assists for NVP2.
    /// </summary>
    [Serializable]
    public partial class ArcadeModule : VehicleModule
    {
        // Steer assist
        
        /// <summary>
        /// Torque that will be applied to the Rigidbody to try and reach the steering angle,
        /// irrelevant of the tire slip. Also works in air.
        /// </summary>
        [UnityEngine.Tooltip("Torque that will be applied to the Rigidbody to try and reach the steering angle,\r\nirrelevant of the tire slip. Also works in air.")]
        public float artificialSteerTorque = 50f;
        
        /// <summary>
        /// Defines artificial steer strength.
        /// </summary>
        [UnityEngine.Tooltip("Defines artificial steer strength.")]
        public float artificialSteerStrength    = 0.1f;

        // Drift assist
        
        /// <summary>
        /// Strength of drift assist.
        /// </summary>
        [UnityEngine.Tooltip("Strength of drift assist.")]
        public float driftAssistStrength            = 1f;
        
        /// <summary>
        /// Angle that the vehicle will attempt to hold when drifting.
        /// Force is applied if the angle goes over this value. If the angle is below the drift angle, no force is applied.
        /// </summary>
        [UnityEngine.Tooltip("Angle that the vehicle will attempt to hold when drifting.\r\nForce is applied if the angle goes over this value. If the angle is below the drift angle, no force is applied.")]
        public float targetDriftAngle       = 45f;
        
        /// <summary>
        /// Angle that will be added to targetDriftAngle based on the steering input.
        /// If the vehicle is drifting and there is steering input, drift angle will increase.
        /// </summary>
        [UnityEngine.Tooltip("Angle that will be added to targetDriftAngle based on the steering input.\r\nIf the vehicle is drifting and there is steering input, drift angle will increase.")]
        public float steerAngleContribution = 10f;
        
        /// <summary>
        /// Maximim force that will be applied to the rear axle to keep the vehicle at or below the target drift angle.
        /// </summary>
        [UnityEngine.Tooltip("Maximim force that will be applied to the rear axle to keep the vehicle at or below the target drift angle.")]
        public float maxDriftAssistForce    = 40f;
        
        private float _steerAngleDiff;
        private float _driftAngle;
        private float _prevSteerAngleDiff;
        private float _prevDriftError;

        /// <summary>
        /// Current drift angle of the vehicle.
        /// </summary>
        public float DriftAngle => _driftAngle;
        
        public override void Initialize()
        {
            base.Initialize();
        }


        public override void Update()
        {
            if (!Active)
            {
                return;
            }

            
        }


        public override void FixedUpdate()
        {
            if (!Active || !vc.IsGrounded() || vc.SpeedSigned < 1f)
            {
                return;
            }

            // Steer assist
            _steerAngleDiff = VehicleController.CalculateSignedCentralAngle(vc.vehicleRigidbody.velocity.normalized, vc.vehicleTransform.forward,
                   vc.vehicleTransform.up);
            _steerAngleDiff = Mathf.Clamp(_steerAngleDiff, -45f, 45f);

            float steerAngleDiffIntegral = (_steerAngleDiff - _prevSteerAngleDiff) / Time.fixedDeltaTime;
            float steerTorque = -(_steerAngleDiff + steerAngleDiffIntegral) * artificialSteerTorque * artificialSteerStrength;
            steerTorque *= Mathf.Clamp01(vc.Speed / 5f);
            vc.vehicleNRigidbody.AddTorque(new Vector3(0, steerTorque, 0), false);
            _prevSteerAngleDiff = _steerAngleDiff;


            // Drift assist
            if (vc.powertrain.wheelGroups.Count != 2) return;

            Vector3 normVel = vc.vehicleRigidbody.velocity.normalized;
            Vector3 vehicleDir = vc.transform.forward;
            _driftAngle = VehicleController.CalculateSignedCentralAngle(normVel, vehicleDir, vc.transform.up);
            _driftAngle = Mathf.Sign(_driftAngle)
                       * Mathf.Clamp(Mathf.Abs(Mathf.Clamp(_driftAngle, -90f, 90f)), 0f, Mathf.Infinity);

            WheelGroup a = vc.WheelGroups[1];
            WheelComponent leftWheel = a.LeftWheel;
            WheelComponent rightWheel = a.RightWheel;

            Vector3 center =
                (leftWheel.ControllerTransform.position + rightWheel.ControllerTransform.position) * 0.5f;

            float driftAngleAimed = targetDriftAngle + vc.input.Steering * steerAngleContribution;
            float error = _driftAngle - driftAngleAimed;
            float absError = Mathf.Abs(_driftAngle) - Mathf.Abs(driftAngleAimed);
            float absErrorIntegral = (absError - _prevDriftError) / Time.fixedDeltaTime;
            Vector3 force = 
                Mathf.Clamp(absError + absErrorIntegral, 0, 90) * Mathf.Sign(_driftAngle) * vc.transform.right * maxDriftAssistForce;
            force *= Mathf.Clamp01(vc.Speed / 3f);
            vc.vehicleNRigidbody.AddForceAtPosition(force, center, false);
            _prevDriftError = absError;

        }


        public override ModuleCategory GetModuleCategory()
        {
            return ModuleCategory.Control;
        }
    }
}