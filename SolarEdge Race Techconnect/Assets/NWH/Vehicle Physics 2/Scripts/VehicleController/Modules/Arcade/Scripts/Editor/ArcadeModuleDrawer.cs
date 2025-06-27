#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;
using UnityEngine;

namespace NWH.VehiclePhysics2.Modules.ArcadeModule
{
    [CustomPropertyDrawer(typeof(ArcadeModule))]
    public partial class ArcadeModuleDrawer : ModuleDrawer
    {
        public override bool OnNUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!base.OnNUI(position, property, label))
            {
                return false;
            }
            
            ArcadeModule moduleTemplate = SerializedPropertyHelper.GetTargetObjectOfProperty(property) as ArcadeModule;
            if (moduleTemplate == null)
            {
                drawer.EndProperty();
                return false;
            }
            
            drawer.BeginSubsection("Artificial Steer");
            drawer.Field("artificialSteerStrength", true, "x100%");
            drawer.Field("artificialSteerTorque", true, "Nm");
            drawer.EndSubsection();
            
            drawer.BeginSubsection("Drift Assist");
            drawer.Field("driftAssistStrength", true, "x100%");
            drawer.Field("targetDriftAngle", true, "deg");
            drawer.Field("steerAngleContribution", true, "deg");
            drawer.Field("maxDriftAssistForce", true, "N");
            drawer.EndSubsection();

            drawer.EndProperty();
            return true;
        }
    }
}

#endif
