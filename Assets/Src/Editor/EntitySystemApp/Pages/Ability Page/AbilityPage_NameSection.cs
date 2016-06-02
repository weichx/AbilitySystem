using UnityEditor;
using UnityEngine;

public class AbilityPage_NameSection : SectionBase<Ability> {

    public AbilityPage_NameSection(float spacing) : base(spacing) {}

    public override void Render() {
        if (rootProperty == null) return;
        SerializedPropertyX id = rootProperty.FindProperty("id");
        SerializedPropertyX icon = rootProperty.FindProperty("icon");
        GUILayout.BeginHorizontal();
        EditorGUILayoutX.PropertyField(icon, GUIContent.none, false, GUILayout.Width(64f), GUILayout.Height(64f));

        GUILayout.BeginVertical();
        GUILayout.Space(20f);
        EditorGUILayoutX.PropertyField(id, new GUIContent("Ability Name"), true);
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