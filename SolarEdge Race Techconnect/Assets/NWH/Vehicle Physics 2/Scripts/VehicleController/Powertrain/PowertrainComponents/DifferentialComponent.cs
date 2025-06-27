using System;
using System.Collections.Generic;
using UnityEngine;

namespace NWH.VehiclePhysics2.Powertrain
{
    [Serializable]
    public partial class DifferentialComponent : PowertrainComponent
    {
        public delegate void SplitTorque(float T, float Wa, float Wb, float Ia, float Ib, float dt, float biasAB,
            float stiffness, float powerRamp, float coastRamp, float slipTorque, out float Ta, out float Tb);

        public enum Type
        {
            Open,
            Locked,
            LimitedSlip,
            VLSD_Obsolete_Placeholder, // TODO - remove in the future
            External
        }

        /// <summary>
        ///     Torque bias between left (A) and right (B) output in [0,1] range.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("    Torque bias between left (A) and right (B) output in [0,1] range.")]
        public float biasAB = 0.5f;

        [Range(0, 1)]
        public float coastRamp = 0.5f;

        /// <summary>
        ///     Differential type.
        /// </summary>
        [ShowInSettings("Differential Type")]
        [Tooltip("    Differential type.")]
        public Type differentialType;

        /// <summary>
        ///     Second output of differential.
        /// </summary>
        [Tooltip("    Second output of differential.")]
        public PowertrainComponent outputB;

        [SerializeField]
        [Range(0, 1)]
        public float powerRamp = 1f;

        /// <summary>
        ///     Slip torque of limited slip differentials.
        /// </summary>
        [SerializeField]
        [Tooltip("    Slip torque of limited slip differentials.")]
        public float slipTorque = 5000f;

        public SplitTorque splitTorqueDelegate;

        /// <summary>
        ///     Stiffness of locking differential [0,1]. Higher value
        ///     will result in lower difference in rotational velocity between left and right wheel.
        ///     Too high value might introduce slight oscillation due to drivetrain windup and a vehicle that is hard to steer.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip(
            "Stiffness of locking differential [0,1]. Higher value\r\nwill result in lower difference in rotational velocity between left and right wheel." +
            "\r\nToo high value might introduce slight oscillation due to drivetrain windup.")]
        public float stiffness = 0.5f;

        [SerializeField]
        protected OutputSelector outputBSelector = new OutputSelector();

        private bool _outputBIsNull;

        private SplitTorque cHLSDTorqueSplitDelegate;
        private SplitTorque cLockingTorqueSplitDelegate;
        private SplitTorque cOpenTorqueSplitDelegate;
        private SplitTorque cLimitedTorqueSplitDelegate;


        public DifferentialComponent()
        {
        }


        public DifferentialComponent(string name, PowertrainComponent outputA, PowertrainComponent outputB)
        {
            this.name    = name;
            this.outputA = outputA;
            this.outputB = outputB;
        }


        public override void Initialize(VehicleController vc)
        {
            base.Initialize(vc);

            if (differentialType == Type.VLSD_Obsolete_Placeholder) differentialType = Type.LimitedSlip;

            _outputBIsNull = outputB == null;

            cOpenTorqueSplitDelegate    = OpenDiffTorqueSplit;
            cLockingTorqueSplitDelegate = LockingDiffTorqueSplit;
            cLimitedTorqueSplitDelegate    = LimitedDiffTorqueSplit;
        }


        public override void OnPrePhysicsSubstep(float t, float dt)
        {
            base.OnPrePhysicsSubstep(t, dt);

            _outputBIsNull = outputB == null;

            if (differentialType == Type.Open)
            {
                splitTorqueDelegate = cOpenTorqueSplitDelegate;
            }
            else if (differentialType == Type.Locked)
            {
                splitTorqueDelegate = cLockingTorqueSplitDelegate;
            }
            else if (differentialType == Type.LimitedSlip)
            {
                splitTorqueDelegate = cLimitedTorqueSplitDelegate;
            }

            // No delegate assigned from external script, fallback to default.
            if (splitTorqueDelegate == null)
            {
                differentialType    = Type.Open;
                splitTorqueDelegate = cOpenTorqueSplitDelegate;
            }
        }


        public override void SetDefaults(VehicleController vc)
        {
            base.SetDefaults(vc);

            name = "Differential";
            inertia = 0.02f;
        }



        public override void FindOutputs(Powertrain powertrain)
        {
            if (string.IsNullOrEmpty(outputASelector.name) || string.IsNullOrEmpty(outputBSelector.name))
            {
                return;
            }

            PowertrainComponent oa = powertrain.GetComponent(outputASelector.name);
            if (oa == null)
            {
                Debug.LogError($"Unknown component '{outputASelector.name}'");
                return;
            }

            outputA = oa;

            PowertrainComponent ob = powertrain.GetComponent(outputBSelector.name);
            if (ob == null)
            {
                Debug.LogError($"Unknown component '{outputBSelector.name}'");
                return;
            }

            outputB = ob;
        }


        public override void GetAllOutputs(ref List<PowertrainComponent> outputs)
        {
            outputs.Clear();
            outputs.Add(outputA);
            outputs.Add(outputB);
        }


        public void LimitedDiffTorqueSplit(float T, float Wa, float Wb, float Ia, float Ib, float dt, float biasAB,
            float stiffness, float powerRamp, float coastRamp, float slipTorque, out float Ta, out float Tb)
        {
            if (Wa < 0 || Wb < 0)
            {
                Ta = T * (1f - biasAB);
                Tb = T * biasAB;
                return;
            }

            //Wa and Wb are positive at this point
            float c = T > 0 ? powerRamp : coastRamp;

            float tFactor = (T < 0 ? -T : T) / slipTorque;
            tFactor = tFactor < -1f ? -1f : tFactor > 1f ? 1f : tFactor;

            float Wtotal = (Wa < 0 ? -Wa : Wa) + (Wb < 0 ? -Wb : Wb);
            float slip = Wa == 0 || Wb == 0 ? 0 : (Wa - Wb) / Wtotal;
            float Td = slip * stiffness * c * slipTorque * tFactor;

            Ta = T * (1f - biasAB) - Td;
            Tb = T * biasAB + Td;
        }


        public void LockingDiffTorqueSplit(float T, float Wa, float Wb, float Ia, float Ib, float dt, float biasAB,
            float stiffness, float powerRamp, float coastRamp, float slipTorque, out float Ta, out float Tb)
        {
            float Isum = Ia + Ib;

            float W            = Ia / Isum * Wa + Ib / Isum * Wb;
            float TaCorrective = (W - Wa) * Ia / dt;
            TaCorrective *= stiffness;
            float TbCorrective = (W - Wb) * Ib / dt;
            TbCorrective *= stiffness;
            
            float Tabs = T < 0 ? -T : T;
            TbCorrective = TbCorrective > 0 ? 
                (TbCorrective > Tabs ? Tabs : TbCorrective) :
                (TbCorrective < -Tabs ? -Tabs : TbCorrective);

            float biasA = 0.5f + (Wb - Wa) * 0.1f * stiffness;
            biasA = biasA < 0 ? 0 : biasA > 1f ? 1f : biasA;

            Ta = T * biasA + TaCorrective;
            Tb = T * (1f - biasA) + TbCorrective;
        }


        public void OpenDiffTorqueSplit(float T, float Wa, float Wb, float Ia, float Ib, float dt, float biasAB,
            float stiffness, float powerRamp, float coastRamp, float slipTorque, out float Ta, out float Tb)
        {
            Ta = T * (1f - biasAB);
            Tb = T * biasAB;
        }


        public override float QueryAngularVelocity(float inputAngularVelocity, float dt)
        {
            angularVelocity = inputAngularVelocity;
            if (_outputAIsNull || _outputBIsNull)
            {
                return inputAngularVelocity;
            }

            float Wa = outputA.QueryAngularVelocity(inputAngularVelocity, dt);
            float Wb = outputB.QueryAngularVelocity(inputAngularVelocity, dt);
            float W  = (Wa + Wb) * 0.5f;

            return W;
        }


        public override float QueryInertia()
        {
            if (_outputAIsNull || _outputBIsNull)
            {
                return inertia;
            }

            float Ia = outputA.QueryInertia();
            float Ib = outputB.QueryInertia();
            float I  = inertia + (Ia + Ib);
            return I;
        }


        public override float ForwardStep(float torque, float inertiaSum, float t, float dt, int i)
        {
            if (_outputAIsNull || _outputBIsNull)
            {
                return torque;
            }

            float Wa = outputA.QueryAngularVelocity(angularVelocity, dt);
            float Wb = outputB.QueryAngularVelocity(angularVelocity, dt);

            float Ia = outputA.QueryInertia();
            float Ib = outputB.QueryInertia();

            splitTorqueDelegate.Invoke(torque, Wa, Wb, Ia, Ib, dt, biasAB, stiffness, powerRamp,
                                       coastRamp, slipTorque, out float Ta, out float Tb);

            return outputA.ForwardStep(Ta, inertiaSum * 0.5f + Ia, t, dt, i)
                   + outputB.ForwardStep(Tb, inertiaSum * 0.5f + Ib, t, dt, i);
        }


        public void SetOutput(PowertrainComponent outputAComponent, PowertrainComponent outputBComponent)
        {
            if (string.IsNullOrEmpty(outputAComponent.name) || string.IsNullOrEmpty(outputBComponent.name))
            {
                Debug.LogWarning("Trying to set powertrain component output to a nameless component. " +
                                 "Output will be set to [none]");
            }

            SetOutput(outputAComponent.name, outputBComponent.name);
        }


        public void SetOutput(string outputAName, string outputBName)
        {
            if (string.IsNullOrEmpty(outputAName))
            {
                outputASelector.name = "[none]";
            }
            else
            {
                outputASelector.name = outputAName;
            }

            if (string.IsNullOrEmpty(outputBName))
            {
                outputBSelector.name = "[none]";
            }
            else
            {
                outputBSelector.name = outputBName;
            }
        }
    }
}