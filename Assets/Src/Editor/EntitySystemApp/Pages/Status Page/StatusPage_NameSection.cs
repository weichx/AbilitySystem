using UnityEngine;
using UnityEditor;

public class StatusPage_NameSection : StatusPage_SectionBase {

    public override void Render() {
        if (serialRoot == null) return;
        SerializedProperty iconProp = statusProperty.FindPropertyRelative("icon");
        SerializedProperty nameProp = statusProperty.FindPropertyRelative("id");
        GUILayout.BeginHorizontal();
        iconProp.objectReferenceValue = EditorGUILayout.ObjectField(iconProp.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(64f), GUILayout.Height(64f));
        GUILayout.BeginVertical();
        GUILayout.Space(20f);
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 150f;
        EditorGUILayout.PropertyField(nameProp, new GUIContent("Status Effect Name"));
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
