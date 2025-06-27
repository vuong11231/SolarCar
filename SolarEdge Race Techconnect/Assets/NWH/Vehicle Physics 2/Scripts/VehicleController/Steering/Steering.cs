using System;
using NWH.VehiclePhysics2.Powertrain;
using NWH.VehiclePhysics2.Powertrain.Wheel;
using NWH.WheelController3D;
using UnityEngine;

namespace NWH.VehiclePhysics2
{
    /// <summary>
    ///     Controls vehicle's steering and steering geometry.
    /// </summary>
    [Serializable]
    public partial class Steering : VehicleComponent
    {
        /// <summary>
        ///     Only used if limitSteeringRate is true. Will limit wheels so that they can only steer up to the set degree
        ///     limit per second. E.g. 60 degrees per second will mean that the wheels that have 30 degree steer angle will
        ///     take 1 second to steer from full left to full right.
        /// </summary>
        [Tooltip(
            "Only used if limitSteeringRate is true.Will limit wheels so that they can only steer up to the set degree" +
            "limit per second. E.g. 60 degrees per second will mean that the wheels that have 30 degree steer angle will" +
            "take 1 second to steer from full left to full right.")]
        [ShowInSettings("Deg/s Limit", 50f, 500f, 20f)]
        public float degreesPerSecondLimit = 180f;

        /// <summary>
        ///     If true direct steering input will be used, without any modification.
        /// </summary>
        [Tooltip("    If true direct steering input will be used, without any modification.")]
        public bool useDirectInput;

        public AnimationCurve linearity = new AnimationCurve(
            new Keyframe(0, 0, 1, 1),
            new Keyframe(1, 1, 1, 1)
        );

        /// <summary>
        ///     Maximum steering angle at the wheels.
        /// </summary>
        [Range(0f, 90f)]
        [ShowInSettings("Max. Steer Angle", 10f, 50f, 2f)]
        [Tooltip("    Maximum steering angle at the wheels.")]
        public float maximumSteerAngle = 25f;

        /// <summary>
        ///     Should wheels return to neutral position when there is no input?
        /// </summary>
        [ShowInSettings("Return To Center")]
        [Tooltip("    Should wheels return to neutral position when there is no input?")]
        public bool returnToCenter = true;

        /// <summary>
        ///     Curve that shows how the steering angle behaves at certain speed.
        ///     X axis represents velocity in range 0 to 100m/s (normalized to 0,1).
        ///     Y axis represents 0 to maximumSteerAngle (normalized to 0,1).
        /// </summary>
        [Tooltip(
            "Curve that shows how the steering angle behaves at certain speed.\r\nX axis represents velocity in range 0 to 100m/s (normalized to 0,1).\r\nY axis represents 0 to maximumSteerAngle (normalized to 0,1).")]
        public AnimationCurve speedSensitiveSteeringCurve = new AnimationCurve(
            new Keyframe(0f,   1f),
            new Keyframe(0.3f, 0.6f, 0f,   -0.6f),
            new Keyframe(1f,   0.1f, 0.5f, 0f)
        );

        public AnimationCurve speedSensitiveSmoothingCurve = new AnimationCurve(
           new Keyframe(0f, 0.05f),
           new Keyframe(1f, 0.15f)
       );

        /// <summary>
        ///     Steering wheel transform that will be rotated when steering. Optional.
        /// </summary>
        [Tooltip("    Steering wheel transform that will be rotated when steering. Optional.")]
        public Transform steeringWheel;

        /// <summary>
        ///     Steer angle will be multiplied by this value to get steering wheel angle. Ignored if steering wheel is null.
        ///     If you want the steering wheel to rotate in opposite direction use negative value.
        /// </summary>
        [Tooltip(
            "Steer angle will be multiplied by this value to get steering wheel angle. Ignored if steering wheel is null.\r\nIf you want the steering wheel to rotate in opposite direction use negative value.")]
        public float steeringWheelTurnRatio = 5f;

        private Vector3 _initialSteeringWheelRotation;
        private float   _steerVelocity;
        private float   _targetAngle;

        /// <summary>
        ///     Current steer angle.
        /// </summary>
        public float Angle { get; set; }

        /// <summary>
        ///     Angle added to the user set angle, used mostly for motorcycle balancing.
        ///     To add angle to the current steer angle use this instead of Angle, since this goes around smoothing and clamping.
        /// </summary>
        public float AdditionalAngle { get; set; }

        public override void Initialize()
        {
            if (steeringWheel != null)
            {
                _initialSteeringWheelRotation = steeringWheel.transform.localRotation.eulerAngles;
            }

            base.Initialize();
        }


        public override void Awake(VehicleController vc)
        {
            base.Awake(vc);
            _targetAngle   = 0;
            _steerVelocity = 0;
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

            CalculateSteerAngles();
            VisualUpdate();
        }

        public virtual void CalculateSteerAngles()
        {
            
            float horizontalInput = vc.input.Steering;
            float smoothing = speedSensitiveSmoothingCurve.Evaluate(vc.Speed / 50f);
            if (!useDirectInput && !returnToCenter && horizontalInput > -0.04f && horizontalInput < 0.04f)
            {
                return;
            }

            if (useDirectInput)
            {
                Angle = horizontalInput * maximumSteerAngle;
            }
            else
            {
                float absHorizontalInput  = horizontalInput < 0 ? -horizontalInput : horizontalInput;
                float horizontalInputSign = horizontalInput < 0 ? -1 : 1;
                float maxAngle            = speedSensitiveSteeringCurve.Evaluate(vc.Speed / 50f) * maximumSteerAngle;
                float inputAngle          = maxAngle * linearity.Evaluate(absHorizontalInput) * horizontalInputSign;
                _targetAngle = Mathf.SmoothDamp(_targetAngle, inputAngle, ref _steerVelocity, smoothing);
                Angle        = Mathf.MoveTowards(Angle, _targetAngle, degreesPerSecondLimit * vc.fixedDeltaTime);
            }

            foreach (WheelGroup wheelGroup in vc.WheelGroups)
            {
                if (wheelGroup.casterAngle != 0 || wheelGroup.toeAngle != 0)
                {
                    foreach (WheelComponent wheel in wheelGroup.Wheels)
                        if (wheel.wheelController.VehicleSide == WheelController.Side.Left)
                        {
                            wheel.ControllerTransform.localEulerAngles = new Vector3(
                                -wheelGroup.casterAngle,
                                wheelGroup.toeAngle,
                                wheel.ControllerTransform.localEulerAngles.z);
                        }
                        else if (wheel.wheelController.VehicleSide == WheelController.Side.Right)
                        {
                            wheel.ControllerTransform.localEulerAngles = new Vector3(
                                -wheelGroup.casterAngle,
                                -wheelGroup.toeAngle,
                                wheel.ControllerTransform.localEulerAngles.z);
                        }
                }

                float axleAngle = (Angle + AdditionalAngle) * wheelGroup.steerCoefficient;

                // Apply Ackermann
                if (wheelGroup.Wheels.Count == 2)
                {
                    if (vc.wheelbase > 0.001f && wheelGroup.addAckerman) // Wheelbase will be larger than 0 only when 4 wheels, 2 per axle.
                    {
                        float axleAngleRad = axleAngle * Mathf.Deg2Rad;
                        float sinAxleAngle = Mathf.Sin(axleAngleRad);
                        float cosAxleAngle = Mathf.Cos(axleAngleRad);

                        float fiInner = Mathf.Atan(4 * wheelGroup.trackWidth * sinAxleAngle /
                                                   (2 * vc.wheelbase * cosAxleAngle - wheelGroup.trackWidth * sinAxleAngle));

                        float fiOuter = Mathf.Atan(4 * wheelGroup.trackWidth * sinAxleAngle /
                                                   (2 * vc.wheelbase * cosAxleAngle + wheelGroup.trackWidth * sinAxleAngle));

                        if (axleAngle < 0)
                        {
                            wheelGroup.RightWheel.SteerAngle = fiInner * Mathf.Rad2Deg;
                            wheelGroup.LeftWheel.SteerAngle  = fiOuter * Mathf.Rad2Deg;
                        }
                        else
                        {
                            wheelGroup.LeftWheel.SteerAngle  = fiOuter * Mathf.Rad2Deg;
                            wheelGroup.RightWheel.SteerAngle = fiInner * Mathf.Rad2Deg;
                        }
                    }


                    // Detoriate handling when damaged
                    if (vc.damageHandler.IsEnabled && !vc.damageHandler.visualOnly)
                    {
                        wheelGroup.LeftWheel.SteerAngle += wheelGroup.LeftWheel.Damage * maximumSteerAngle *
                                                     wheelGroup.LeftWheel.DamageSteerDirection;
                        wheelGroup.RightWheel.SteerAngle += wheelGroup.RightWheel.Damage * maximumSteerAngle *
                                                      wheelGroup.RightWheel.DamageSteerDirection;
                    }
                }
                else
                {
                    foreach (WheelComponent wheel in wheelGroup.Wheels) wheel.SteerAngle = axleAngle;
                }
            }
        }

        public virtual void VisualUpdate()
        {
            // Adjust steering wheel object if it exists
            if (steeringWheel != null)
            {
                float wheelAngle = Angle * steeringWheelTurnRatio;
                steeringWheel.transform.localRotation = Quaternion.Euler(_initialSteeringWheelRotation);
                steeringWheel.transform.Rotate(Vector3.forward, wheelAngle);
            }
        }

        public override void SetDefaults(VehicleController vc)
        {
            base.SetDefaults(vc);

            speedSensitiveSteeringCurve = new AnimationCurve(
                new Keyframe(0f,   1f,   0f,    0f),
                new Keyframe(0.3f, 0.4f, -0.6f, -0.6f),
                new Keyframe(1f,   0.2f, -0.1f, 0.1f)
            );

            linearity = new AnimationCurve(
                new Keyframe(0, 0, 1, 1),
                new Keyframe(1, 1, 1, 1));
        }
    }
}