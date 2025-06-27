using System;
using NWH.Common;
using NWH.Common.Utility;
using UnityEngine;

namespace NWH.VehiclePhysics2.Powertrain
{
    [Serializable]
    public partial class ClutchComponent : PowertrainComponent
    {
        /// <summary>
        ///     RPM at which automatic clutch will try to engage.
        /// </summary>
        [ShowInSettings("Base Engagement RPM", 1000f, 4000f, 200f)]
        [Tooltip("    RPM at which automatic clutch will try to engage.")]
        public float baseEngagementRPM = 1200f;

        /// <summary>
        ///     Clutch engagement in range [0,1] where 1 is fully engaged clutch.
        ///     Affected by Slip Torque field as the clutch can transfer [clutchEngagement * slipTorque] Nm
        ///     meaning that higher value of slipTorque will result in more sensitive clutch.
        /// </summary>
        [Range(0, 1)]
        [ShowInTelemetry]
        [Tooltip(
            "Clutch engagement in range [0,1] where 1 is fully engaged clutch.\r\nAffected by Slip Torque field as the clutch can transfer [clutchEngagement * slipTorque] Nm\r\nmeaning that higher value of slipTorque will result in more sensitive clutch.")]
        public float clutchEngagement;

        /// <summary>
        /// Curve representing pedal travel vs. clutch engagement. Should start at 0,0 and end at 1,1.
        /// </summary>
        [Tooltip("Curve representing pedal travel vs.clutch engagement. Should start at 0,0 and end at 1,1.")]
        public AnimationCurve                       clutchEngagementCurve = new AnimationCurve();

        /// <summary>
        ///     RPM at which the clutch will engage. Equals baseEngagementRPM plus variable engagement range.
        /// </summary>
        [ShowInTelemetry]
        [Tooltip("    RPM at which the clutch will engage. Equals baseEngagementRPM plus variable engagement range.")]
        public float finalEngagementRPM = 2000f;

        public int gear;

        /// <summary>
        ///     Is the clutch automatic? If true any input set manually will be overridden by the result given by PID controller
        ///     based on the difference between engine and clutch RPM.
        /// </summary>
        [ShowInSettings]
        [Tooltip(
            "Is the clutch automatic? If true any input set manually will be overridden by the result given by PID controller\r\nbased on the difference between engine and clutch RPM.")]
        public bool isAutomatic = true;

        /// <summary>
        ///     Final result of PID controller is multiplied by this value. Used to adjust how fast PID reacts without
        ///     having to change individual coefficients.
        /// </summary>
        [Tooltip(
            "Final result of PID controller is multiplied by this value. Used to adjust how fast PID reacts without\r\nhaving to change individual coefficients.")]
        public float engagementSpeed = 1f;

        /// <summary>
        ///     Derivative term of automatic clutch PID controller. Clutch engagement is adjusted based
        ///     on speed of change of the error between the clutch RPM and engine RPM.
        ///     Too high value will result in oscillation.
        /// </summary>
        [SerializeField]
        [Range(0f, 0.4f)]
        [Tooltip(
            "Derivative term of automatic clutch PID controller.")]
        public float PID_Kd = 0.1f;

        /// <summary>
        ///     Integral term of automatic clutch PID controller. Clutch engagement is adjusted based on
        ///     how long the error between the clutch RPM and engine RPM has persisted.
        ///     Too high value will result in oscillation.
        /// </summary>
        [SerializeField]
        [Range(0f, 5f)]
        [Tooltip(
            "Integral term of automatic clutch PID controller.")]
        public float PID_Ki = 2.4f;

        /// <summary>
        ///     Proportional term of automatic clutch PID controller. Clutch engagement is adjusted
        ///     proportionally to the error between the clutch RPM and engine RPM.
        ///     Low value will result in clutch engaging very slowly while high value will result in
        ///     clutch engaging fast - possibly stalling the engine in extreme cases.
        /// </summary>
        [SerializeField]
        [Range(0f, 5f)]
        [Tooltip(
            "Proportional term of automatic clutch PID controller.")]
        public float PID_Kp = 4f;

        /// <summary>
        /// Transmission shift flag.
        /// </summary>
        public bool shiftSignal;

        /// <summary>
        /// Engine start flag.
        /// </summary>
        public bool startSignal;

        /// <summary>
        ///     Torque at which the clutch will slip / maximum torque that the clutch can transfer.
        ///     This value also affects clutch engagement as higher slip value will result in clutch
        ///     that grabs higher up / sooner. Too high slip torque value combined with low inertia of
        ///     powertrain components might cause instability in powertrain solver.
        /// </summary>
        [SerializeField]
        [ShowInSettings("Slip Torque", 100f, 5000f, 200f)]
        [Tooltip(
            "Torque at which the clutch will slip / maximum torque that the clutch can transfer.\r\nThis value also affects clutch engagement as higher slip value will result in clutch\r\nthat grabs higher up / sooner. Too high slip torque value combined with low inertia of\r\npowertrain components might cause instability in powertrain solver.")]
        public float slipTorque = 500f;

        /// <summary>
        /// Amount of torque that will be passed through clutch even when completely disengaged
        /// to emulate torque converter creep on automatic transmissions.
        /// Should be higher than rolling resistance of the wheels to get the vehicle rolling.
        /// </summary>
        [Tooltip("Amount of torque that will be passed through clutch even when completely disengaged to emulate torque converter creep on automatic transmissions." +
                 "Should be higher than rolling resistance of the wheels to get the vehicle rolling.")]
        public float creepTorque = 0;
        
        /// <summary>
        ///     Maximum RPM value that variableEngagementIntensity field can add to engagement RPM.
        ///     Final clutch engagement RPM is calculated by multiplying this value by variableEngagementIntensity and adding the
        ///     result
        ///     engagementRPM.
        /// </summary>
        [SerializeField]
        [Tooltip(
            "Maximum RPM value that variableEngagementIntensity field can add to engagement RPM.\r\nFinal clutch engagement RPM is calculated by multiplying this value by variableEngagementIntensity and adding the result\r\nengagementRPM. ")]
        public float variableEngagementRPMRange = 1400f;

        public float variableEngagementTPS;

        private float _cachedTargetAngVel;
        private float _e, _ePrev;
        private float _ed;
        private float _ei;


        public override void OnPrePhysicsSubstep(float t, float dt)
        {
            base.OnPrePhysicsSubstep(t, dt);

            finalEngagementRPM  = baseEngagementRPM + variableEngagementTPS * variableEngagementRPMRange;
            _cachedTargetAngVel = UnitConverter.RPMToAngularVelocity(finalEngagementRPM);
        }


        public override void Initialize(VehicleController vc)
        {
            base.Initialize(vc);

            if (clutchEngagementCurve.keys.Length < 2)
            {
                SetDefaultClutchEngagementCurve();
            }
        }


        public override void OnDisable()
        {
            base.OnDisable();

            clutchEngagement = 0;
        }


        public override void SetDefaults(VehicleController vc)
        {
            name    = "Clutch";
            inertia = 0.02f;
            SetDefaultClutchEngagementCurve();
        }


        private void SetDefaultClutchEngagementCurve()
        {
            clutchEngagementCurve = new AnimationCurve(
                new Keyframe[2]
                {
                    new Keyframe(0, 0), 
                    new Keyframe(1, 1)
                }); // Linear clutch engagement
        }

        public override void Validate(VehicleController vc)
        {
            base.Validate(vc);

            Debug.Assert(!string.IsNullOrEmpty(outputASelector.name),
                         "Clutch is not connected to anything. Go to Powertrain > Clutch and set the output.");
        }


        public override float QueryAngularVelocity(float inputAngularVelocity, float dt)
        {
            angularVelocity = inputAngularVelocity;
            if (_outputAIsNull)
            {
                return inputAngularVelocity;
            }

            // Adjust engagement
            if (isAutomatic)
            {
                if (shiftSignal || startSignal)
                {
                    clutchEngagement = 0f;
                }
                else
                {
                    _ePrev =  _e;
                    _e     =  _cachedTargetAngVel - angularVelocity;
                    _ei    += _e * dt;
                    if (_e <= 0)
                    {
                        _ei = 0;
                    }

                    _ed = (_e - _ePrev) / dt;
                    float diff = PID_Kp * _e * dt + PID_Ki * _ei * dt + PID_Kd * _ed * dt;
                    diff             *= engagementSpeed;
                    clutchEngagement -= diff * dt * 30f;
                }

                clutchEngagement = clutchEngagement < 0 ? 0 : clutchEngagement > 1 ? 1 : clutchEngagement;
            }

            // Solver uses velocity based approach which is not ideal for clutch simulation
            float Wout = outputA.QueryAngularVelocity(inputAngularVelocity, dt) * clutchEngagement;
            float Win  = inputAngularVelocity * (1f - clutchEngagement);
            float W    = Wout + Win;
            return W;
        }


        public override float QueryInertia()
        {
            if (_outputAIsNull)
            {
                return inertia;
            }

            float I = inertia + outputA.QueryInertia() * clutchEngagement;
            return I;
        }


        public override float ForwardStep(float torque, float inertiaSum, float t, float dt, int i)
        {
            if (_outputAIsNull)
            {
                return torque;
            }

            torque = torque > slipTorque ? slipTorque : torque < -slipTorque ? -slipTorque : torque;

            float clutchEngagmentCurveValue = clutchEngagementCurve.Evaluate(clutchEngagement);
            float forwardTorque = torque * clutchEngagmentCurveValue;
            float forwardInertia = inertiaSum * clutchEngagement + inertia;
            
            // Apply creep torque to forward torque
            if (creepTorque != 0)
            {
                forwardTorque = forwardTorque <= creepTorque && forwardTorque >= -creepTorque // Check if torque lower than creep torque
                    ? forwardTorque > 0 ? -creepTorque : creepTorque // Apply creep torque with the sign of the input torque
                    : forwardTorque; // If torque above creep torque, ignore
            }

            
            float returnTorque = outputA.ForwardStep(forwardTorque, forwardInertia, t, dt, i); // Forward step the powertrain

            returnTorque *= clutchEngagmentCurveValue;
            
            returnTorque = returnTorque > slipTorque
                ? slipTorque  // Return torque is higher than slip torque, slip the clutch
                : returnTorque < -slipTorque 
                    ? -slipTorque // Return torque is lower than negative slip torque, slip the clutch
                    : returnTorque; // Return torque is within slip bounds, do nothing
   
            // // Apply creep torque to return torque as the torque converter always has some drag when engaged
            // if (creepTorque != 0)
            // {
            //     returnTorque = returnTorque <= creepTorque && returnTorque >= -creepTorque // Check if torque lower than creep torque
            //         ? returnTorque > 0 ? creepTorque : -creepTorque // Apply creep torque with the sign of the input torque
            //         : returnTorque; // If torque above creep torque, ignore
            // }
            
            return returnTorque;
        }
    }
}