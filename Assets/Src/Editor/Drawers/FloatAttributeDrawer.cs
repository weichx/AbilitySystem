using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatRange))]
public class FloatRangeDrawer : PropertyDrawer {

    private bool shown;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {

        pos = new Rect(pos) {
            height = EditorGUIUtility.singleLineHeight
        };
        object target = prop.serializedObject.targetObject;
        FloatRange attr = target.GetType().GetField(prop.name).GetValue(target) as FloatRange;
        label.text += " (" + attr.Value + ")";
        shown = EditorGUI.Foldout(pos, shown, label);
        if (shown) {
            pos.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.indentLevel++;
            attr.BaseValue = EditorGUI.FloatField(pos, new GUIContent("Base Value"), attr.BaseValue);
            pos.y += EditorGUIUtility.singleLineHeight;
            FloatModifier[] modifiers = attr.GetReadOnlyModiferList();
            for (int i = 0; i < modifiers.Length; i++) {
                FloatModifier modifier = modifiers[i];
                string valueStr = "";
                if (modifier.flatBonus != 0) {
                    valueStr += modifier.flatBonus;
                }
                else if (modifier.percentBonus != 0) {
                    valueStr += modifier.percentBonus + " %";
                }
                var keyRect = new Rect(pos) {
                    width = EditorGUIUtility.labelWidth
                };
                var valueRect = new Rect(pos) {
                    x = EditorGUIUtility.labelWidth,
                };
                EditorGUI.LabelField(keyRect, modifier.id);
                EditorGUI.LabelField(valueRect, valueStr);
                pos.y += EditorGUIUtility.singleLineHeight;
            }
  
            EditorGUI.indentLevel--;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        object target = property.serializedObject.targetObject;
        FloatRange attr = target.GetType().GetField(property.name).GetValue(target) as FloatRange;
        if (shown) {
            return EditorGUIUtility.singleLineHeight * (attr.GetReadOnlyModiferList().Length + 2);
        }
        else {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}