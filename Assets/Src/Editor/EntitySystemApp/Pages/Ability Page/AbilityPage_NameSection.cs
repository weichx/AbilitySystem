using UnityEditor;
using UnityEngine;

public class AbilityPage_NameSection : AbilityPage_SectionBase {

    public override void Render() {
        SerializedPropertyX id = targetItem.SerialObjectX.FindProperty("id");
        SerializedPropertyX icon = targetItem.SerialObjectX.FindProperty("icon");
        GUILayout.BeginHorizontal();
        EditorGUILayoutX.PropertyField(icon, GUIContent.none, false, GUILayout.Width(64f), GUILayout.Height(64f));

        GUILayout.BeginVertical();
        GUILayout.Space(20f);
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 100f;
        EditorGUILayoutX.PropertyField(id, new GUIContent("Ability Name"), true);
        EditorGUIUtility.labelWidth = labelWidth;
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Restore")) {
            targetItem.Restore();
            GUIUtility.keyboardControl = 0;
        }
        if (GUILayout.Button("Delete")) {
            targetItem.QueueDelete();
        }
        if (GUILayout.Button("Save")) {
            targetItem.Save();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

}