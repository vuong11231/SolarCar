#if UNITY_EDITOR
using NWH.NUI;
using UnityEditor;
using UnityEngine;

namespace NWH.VehiclePhysics2.Modules.AirSteer
{
    [CustomPropertyDrawer(typeof(AirSteerModule))]
    public partial class AirSteerModuleDrawer : ModuleDrawer
    {
        public override bool OnNUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!base.OnNUI(position, property, label))
            {
                return false;
            }

            AirSteerModule airSteerModule = SerializedPropertyHelper.GetTargetObjectOfProperty(property) as AirSteerModule;
            if (airSteerModule == null)
            {
                drawer.EndProperty();
                return false;
            }

            drawer.Field("yawTorque");
            drawer.Field("pitchTorque");


            drawer.EndProperty();
            return true;
        }
    }
}

#endif
