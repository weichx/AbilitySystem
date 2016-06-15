using UnityEngine;
using UnityEditor;

[PropertyDrawerFor(typeof(IntValue))]
public class IntValueDrawerX : PropertyDrawerX {

    private bool shown;

    public override void OnGUI(SerializedPropertyX property, GUIContent label) {

        IntValue attr = property.GetValue<IntValue>();
        string labelText = property.displayName + " (" + attr.Value + ")";
        if(attr.Value == int.MaxValue || attr.Value == int.MinValue) {
            labelText = property.displayName + " (--)";
        }
        label.text = labelText;
        shown = EditorGUILayout.Foldout(shown, label);
        if (shown) {
            EditorGUI.indentLevel++;
            attr.BaseValue = EditorGUILayout.IntField(new GUIContent("Base Value"), attr.BaseValue);
            property["baseValue"].Value = attr.BaseValue;
            IntRange.IntRangeBoundry rangeBoundry = attr as IntRange.IntRangeBoundry;
            if (rangeBoundry != null) {
                //this code makes sure the parent's value is clamped now that we've updated a range boundry
                rangeBoundry.BaseValue = property["baseValue"].GetValue<int>();
                int parentValue = rangeBoundry.parent.Value;
                property["parent"]["currentValue"].Value = parentValue;
            }
            IntModifier[] modifiers = attr.GetReadOnlyModiferList();
            for (int i = 0; i < modifiers.Length; i++) {
                IntModifier modifier = modifiers[i];
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
        IntValue attr = property.GetValue<IntValue>();
        if (shown) {
            return EditorGUIUtility.singleLineHeight * (attr.GetReadOnlyModiferList().Length + 2);
        }
        else {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}