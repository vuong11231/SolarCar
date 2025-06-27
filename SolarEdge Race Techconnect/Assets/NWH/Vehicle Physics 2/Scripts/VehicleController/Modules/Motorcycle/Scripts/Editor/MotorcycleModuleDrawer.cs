#if UNITY_EDITOR
using NWH.NUI;
using NWH.VehiclePhysics2.Modules;
using UnityEditor;
using UnityEngine;

namespace NWH.VehiclePhysics2.Modules.MotorcycleModule
{
    [CustomPropertyDrawer(typeof(MotorcycleModule))]
    public partial class MotorcycleModuleDrawer : ModuleDrawer
    {
        public override bool OnNUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!base.OnNUI(position, property, label))
            {
                return false;
            }
            
            MotorcycleModule motorcycleModule = SerializedPropertyHelper.GetTargetObjectOfProperty(property) as MotorcycleModule;
            if (motorcycleModule == null)
            {
                drawer.EndProperty();
                return false;
            }

            drawer.BeginSubsection("Steering");
            drawer.Info("Steering settings are set through 'Control > Steering' tab.");
            drawer.EndSubsection();
            
            drawer.BeginSubsection("Lean");
            drawer.Field("leanAngleSlipCoefficient");
            drawer.Field("leanAngleMaxDelta", true, "deg");
            drawer.Field("maxLeanAngle", true, "deg");
            drawer.Field("maxLeanTorque", true, "N");
            
            drawer.IncreaseIndent();
            drawer.BeginSubsection("Lean PID Controller");
            drawer.Field("gainProportional");
            drawer.Field("gainIntegral");
            drawer.Field("gainDerivative");
            drawer.Field("leanPIDCoefficient");
            drawer.EndSubsection();
            drawer.DecreaseIndent();
                
            drawer.EndSubsection();

            drawer.BeginSubsection("Animation");
            drawer.Field("handlebarsTransform");
            drawer.Field("forksTransform");
            drawer.Field("swingarmTransform");
            drawer.EndSubsection();

            drawer.EndProperty();
            return true;
        }
    }
}

#endif
