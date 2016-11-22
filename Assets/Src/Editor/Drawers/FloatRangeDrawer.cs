using UnityEngine;
using UnityEditor;
using EntitySystem;

using System;

[PropertyDrawerFor(typeof(FloatRange))]
public class FloatRangeDrawerX : PropertyDrawerX {

    public override void OnGUI(SerializedPropertyX property, GUIContent label) {
        FloatRange attr = property.GetValue<FloatRange>();
        label.text = property.displayName + " (" + attr.Value + ")";

        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);
        if (property.isExpanded) {
            property["baseValue"].Value = (float)(EditorGUILayout.FloatField(new GUIContent("Base Value"), property["baseValue"].GetValue<float>()));
            if (Mathf.Approximately(attr.BaseValue, 0)) {
                attr.BaseValue = 0;
            }
            attr.BaseValue = property["baseValue"].GetValue<float>();
            property["currentValue"].Value = attr.Value;
            EditorGUI.indentLevel++;
            FloatModifier[] modifiers = attr.GetReadOnlyModiferList();
            for (int i = 0; i < modifiers.Length; i++) {
                FloatModifier modifier = modifiers[i];
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