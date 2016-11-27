using UnityEngine;
using UnityEditor;
using EntitySystem;

[PropertyDrawerFor(typeof(FloatValue))]
public class FloatValueDrawerX : PropertyDrawerX {

    private bool shown;

    public override void OnGUI(SerializedPropertyX property, GUIContent label) {

        FloatValue attr = property.GetValue<FloatValue>();
        string labelText = property.displayName + " (" + attr.Value + ")";
        if (Mathf.Approximately(attr.Value, float.MaxValue) || Mathf.Approximately(attr.Value, float.MinValue)) {
            labelText = property.displayName + " (--)";
        }
        label.text = labelText;
        shown = EditorGUILayout.Foldout(shown, label);
        if (shown) {
            EditorGUI.indentLevel++;
            attr.BaseValue = (float)System.Math.Round(EditorGUILayout.FloatField(new GUIContent("Base Value"), attr.BaseValue), 3);
            if (Mathf.Approximately(attr.BaseValue, 0)) {
                attr.BaseValue = 0;
            }
            property["baseValue"].Value = attr.BaseValue;
            FloatRange.FloatRangeBoundry rangeBoundry = attr as FloatRange.FloatRangeBoundry;
            if (rangeBoundry != null) {
                //this code makes sure the parent's value is clamped now that we've updated a range boundry
                rangeBoundry.BaseValue = property["baseValue"].GetValue<float>();
                float parentValue = rangeBoundry.parent.Value;
                property["parent"]["currentValue"].Value = parentValue;
            }
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
                EditorGUILayout.LabelField(modifier.id);
                EditorGUILayout.LabelField(valueStr);
            }
  
            EditorGUI.indentLevel--;
        }
    }

    public override float GetPropertyHeight(SerializedPropertyX property, GUIContent label) {
        FloatValue attr = property.GetValue<FloatValue>();
        if (shown) {
            return EditorGUIUtility.singleLineHeight * (attr.GetReadOnlyModiferList().Length + 2);
        }
        else {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}