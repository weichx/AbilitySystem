using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatAttribute))]
public class FloatAttributeDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        //EditorGUI.LabelField(position, "Hello");
    }

}