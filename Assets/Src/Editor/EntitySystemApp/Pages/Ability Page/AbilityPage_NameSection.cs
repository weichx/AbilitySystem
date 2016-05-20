using UnityEditor;
using UnityEngine;

public class AbilityPage_NameSection : AbilityPage_SectionBase {

    public override void Render() {
        SerializedProperty iconProp = abilityProperty.FindPropertyRelative("icon");
        SerializedProperty nameProp = abilityProperty.FindPropertyRelative("id");
        GUILayout.BeginHorizontal();
        iconProp.objectReferenceValue = EditorGUILayout.ObjectField(iconProp.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(64f), GUILayout.Height(64f));
        GUILayout.BeginVertical();
        GUILayout.Space(20f);
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 100f;
        EditorGUILayout.PropertyField(nameProp, new GUIContent("Ability Name"));
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