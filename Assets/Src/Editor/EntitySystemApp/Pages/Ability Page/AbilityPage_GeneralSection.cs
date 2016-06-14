using UnityEngine;
using UnityEditor;

public class AbilityPage_GeneralSection : SectionBase<Ability> {

    public AbilityPage_GeneralSection(float spacing) : base(spacing) {}


    public override void Render() {
        if (targetItem == null) return;
        SerializedPropertyX castMode = rootProperty.FindProperty("castMode");
        SerializedPropertyX ignoreGCD = rootProperty.FindProperty("IgnoreGCD");
        SerializedPropertyX castTime = rootProperty.FindProperty("castTime");//.FindProperty("baseValue");
        SerializedPropertyX channelTime = rootProperty.FindProperty("channelTime");//.FindProperty("baseValue");
        SerializedPropertyX channelTicks = rootProperty.FindProperty("channelTicks");//.FindProperty("baseValue");
        SerializedPropertyX charges = rootProperty.FindProperty("charges");

        GUILayout.BeginHorizontal();
        EditorGUILayoutX.PropertyField(castMode, false);
        ignoreGCD.Value = EditorGUILayout.ToggleLeft("Ignore GCD", (bool)ignoreGCD.Value);
        GUILayout.EndHorizontal();

        int castVal = (int) castMode.Value;
        if (castVal != (int)CastMode.Instant) {
            if (castVal != (int)CastMode.Channel) {
                EditorGUI.indentLevel++;
                EditorGUILayoutX.PropertyField(castTime);
                EditorGUI.indentLevel--;
            }
            if (castVal != (int)CastMode.Cast) {
                EditorGUI.indentLevel++;
                EditorGUILayoutX.PropertyField(channelTime);
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel++;
                EditorGUILayoutX.PropertyField(channelTicks);
                EditorGUI.indentLevel--;
                //channelTime.Value = EditorGUILayout.FloatField(new GUIContent("Channel Time"), (float)channelTime.Value);
                //channelTicks.Value = EditorGUILayout.IntField(new GUIContent("Channel Ticks"), (int)channelTicks.Value);
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
            SerializedPropertyX cooldown = chargeProp.FindProperty("cooldown").FindProperty("baseValue");
            cooldown.Value = EditorGUILayout.FloatField(new GUIContent("Cooldown"), (float)cooldown.Value);
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