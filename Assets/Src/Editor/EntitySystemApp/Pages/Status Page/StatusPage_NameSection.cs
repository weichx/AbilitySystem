using UnityEngine;
using UnityEditor;

public class StatusPage_NameSection : StatusPage_SectionBase {

    public StatusPage_NameSection(StatusPage page) : base(page) { }

    public override void Render() {
        if (target == null) return;
        SerializedProperty iconProp = statusProperty.FindPropertyRelative("icon");
        SerializedProperty nameProp = statusProperty.FindPropertyRelative("statusEffectId");
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
        //GUI.enabled = false;
        if (GUILayout.Button("Restore")) {
            page.Restore();
            GUIUtility.keyboardControl = 0;
        }
        //GUI.enabled = true;
        if (GUILayout.Button("Delete")) {
            page.Delete();
        }
        if (GUILayout.Button("Save")) {
            page.Save();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

}
