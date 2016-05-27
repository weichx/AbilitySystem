using UnityEngine;
using UnityEditor;
using AbilitySystem;

[CustomPropertyDrawer(typeof(WritableAttribute))]
public class WritableAttributeDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return DrawerUtil.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        WritableAttribute attr = attribute as WritableAttribute;
        GUI.enabled = attr.Result(DrawerUtil.GetTarget(property));
        DrawerUtil.OnGUI(position, property, label);
        GUI.enabled = true;

    }
}
