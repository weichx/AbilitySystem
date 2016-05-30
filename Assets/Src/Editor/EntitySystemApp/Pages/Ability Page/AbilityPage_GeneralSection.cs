using UnityEngine;
using UnityEditor;

public class AbilityPage_GeneralSection : AbilityPage_SectionBase {

    public override void Render() {
        if (targetItem == null) return;
        SerializedPropertyX abilityProperty = targetItem.SerialObjectX.FindProperty("ability");
        SerializedPropertyX castMode = targetItem.SerialObjectX.FindProperty("castMode");
        SerializedPropertyX ignoreGCD = targetItem.SerialObjectX.FindProperty("IgnoreGCD");
        SerializedPropertyX castTime = targetItem.SerialObjectX.FindProperty("castTime");
        SerializedPropertyX channelTime = targetItem.SerialObjectX.FindProperty("channelTime");
        SerializedPropertyX channelTicks = targetItem.SerialObjectX.FindProperty("channelTicks");
        SerializedPropertyX charges = targetItem.SerialObjectX.FindProperty("charges");

        GUILayout.BeginHorizontal();
        EditorGUILayoutX.PropertyField(castMode, false);
        ignoreGCD.Value = EditorGUILayout.ToggleLeft("Ignore GCD", (bool)ignoreGCD.Value);
        GUILayout.EndHorizontal();

        int castVal = (int) castMode.Value;
        if (castVal != (int)CastMode.Instant) {
            if (castVal != (int)CastMode.Channel) {
                EditorGUILayoutX.PropertyField(castTime);
            }
            if (castVal != (int)CastMode.Cast) {
                EditorGUILayoutX.PropertyField(channelTime);
                EditorGUILayoutX.PropertyField(channelTicks);
            }
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Charges (" + charges.ArraySize + ")");
        if (GUILayout.Button("+", GUILayout.Width(25f))) {
            charges.ArraySize++;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        for (int i = 0; i < charges.ArraySize; i++) {
            EditorGUILayout.BeginHorizontal();
            SerializedPropertyX chargeProp = charges.GetChildAt(i);
            EditorGUILayoutX.PropertyField(chargeProp.FindProperty("cooldown"));
            GUI.enabled = charges.ArraySize > 1;
            if (GUILayout.Button("-", GUILayout.Width(25f), GUILayout.Height(15f))) {
                charges.DeleteArrayElementAt(i);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
    }

}