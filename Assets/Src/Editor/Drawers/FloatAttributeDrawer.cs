using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatAttribute))]
public class FloatAttributeDrawer : PropertyDrawer {

	public override void OnGUI (Rect pos, SerializedProperty prop, GUIContent label) {
		EditorGUI.LabelField(pos, "Hello");
    }

}