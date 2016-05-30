using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ResponseCurve))]
public class ResponseCurveDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        base.OnGUI(position, property, label);
    }

}