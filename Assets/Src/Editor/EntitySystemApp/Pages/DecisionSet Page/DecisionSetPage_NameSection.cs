using UnityEngine;
using UnityEditor;

public class DecisionSetPage_NameSection : DecisionSetPage_SectionBase {

    public void Render() {
        if (serialRoot == null) return;
        SerializedProperty nameProp = decisionSetProperty.FindPropertyRelative("id");
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Space(20f);
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 150f;
        EditorGUILayout.PropertyField(nameProp, new GUIContent("Decision Package Name"));
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
		serialRoot.ApplyModifiedProperties();
    }

}