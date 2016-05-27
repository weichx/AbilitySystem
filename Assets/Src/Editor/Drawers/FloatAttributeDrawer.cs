using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatAttribute))]
public class FloatAttributeDrawer : PropertyDrawer {

	public override void OnGUI (Rect pos, SerializedProperty prop, GUIContent label) {
        pos = new Rect(pos) {
            width = Mathf.Min(425f, pos.width)
        };
        EditorGUI.PropertyField(pos, prop.FindPropertyRelative("baseValue"), label);
    }

}