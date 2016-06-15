using UnityEngine;
using UnityEditor;
using System;

[PropertyDrawerFor(typeof(IntRange))]
public class IntRangeDrawerX : PropertyDrawerX {

    public override void OnGUI(SerializedPropertyX property, GUIContent label) {
        IntRange attr = property.GetValue<IntRange>();
        label.text = property.displayName + " (" + attr.Value + ")";

        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);
        if (property.isExpanded) {
            property["baseValue"].Value = EditorGUILayout.IntField(new GUIContent("Base Value"), property["baseValue"].GetValue<int>());
            if (Mathf.Approximately(attr.BaseValue, 0)) {
                attr.BaseValue = 0;
            }
            attr.BaseValue = property["baseValue"].GetValue<int>();
            property["currentValue"].Value = attr.Value;
            EditorGUI.indentLevel++;
            IntModifier[] modifiers = attr.GetReadOnlyModiferList();
            for (int i = 0; i < modifiers.Length; i++) {
                IntModifier modifier = modifiers[i];
                string valueStr = "Flat: " + modifier.flatBonus + " Percent: " + modifier.percentBonus;
                GUI.enabled = false;
                EditorGUILayout.TextField(new GUIContent(modifier.id), valueStr);
                GUI.enabled = true;
            }
            EditorGUILayoutX.PropertyField(property["min"]);
            EditorGUILayoutX.PropertyField(property["max"]);
            EditorGUI.indentLevel--;
        }
    }

    public override float GetPropertyHeight(SerializedPropertyX property, GUIContent label) {
        float slh = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded) {
            PropertyDrawerX minDrawerX = Reflector.GetCustomPropertyDrawerFor(property["min"]);
            PropertyDrawerX maxDrawerX = Reflector.GetCustomPropertyDrawerFor(property["max"]);
            SerializedPropertyX min = property["min"];
            SerializedPropertyX max = property["max"];
            return 2f * slh + (minDrawerX.GetPropertyHeight(min, min.label) + maxDrawerX.GetPropertyHeight(max, max.label));
        }
        else {
            return slh;
        }
    }
}