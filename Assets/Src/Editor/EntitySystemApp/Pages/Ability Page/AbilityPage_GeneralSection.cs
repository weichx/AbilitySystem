using UnityEngine;
using UnityEditor;

public class AbilityPage_GeneralSection : AbilityPage_SectionBase {

    public override void Render() {
        if (serialRoot == null) return;
        SerializedProperty castMode = abilityProperty.FindPropertyRelative("castMode");
        SerializedProperty ignoreGCD = abilityProperty.FindPropertyRelative("IgnoreGCD");
        SerializedProperty castTime = abilityProperty.FindPropertyRelative("castTime");
        SerializedProperty channelTime = abilityProperty.FindPropertyRelative("channelTime");
        SerializedProperty channelTicks = abilityProperty.FindPropertyRelative("channelTicks");
        SerializedProperty charges = abilityProperty.FindPropertyRelative("charges");

        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(castMode, false);
        ignoreGCD.boolValue = EditorGUILayout.ToggleLeft("Ignore GCD", ignoreGCD.boolValue);
        GUILayout.EndHorizontal();

        int castVal = castMode.intValue;
        if (castVal != (int)CastMode.Instant) {
            if (castVal != (int)CastMode.Channel) {
                DrawerUtil.DrawProperty(castTime, typeof(Ability));
            }
            if (castVal != (int)CastMode.Cast) {
                DrawerUtil.DrawProperty(channelTime, typeof(Ability));
                DrawerUtil.DrawProperty(channelTicks, typeof(Ability));

            }
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Charges (" + charges.arraySize + ")");
        if (GUILayout.Button("+", GUILayout.Width(25f))) {
            charges.arraySize++;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        for (int i = 0; i < charges.arraySize; i++) {
            EditorGUILayout.BeginHorizontal();
            SerializedProperty chargeProp = charges.GetArrayElementAtIndex(i);
            DrawerUtil.DrawProperty(chargeProp.FindPropertyRelative("cooldown"), typeof(Charge));
            GUI.enabled = charges.arraySize > 1;
            if (GUILayout.Button("-", GUILayout.Width(25f), GUILayout.Height(15f))) {
                charges.DeleteArrayElementAtIndex(i);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
    }

}