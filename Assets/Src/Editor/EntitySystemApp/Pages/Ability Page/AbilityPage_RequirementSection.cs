using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class AbilityPage_RequirementSection : AbilityPage_SectionBase {

    private bool shown;
    private SearchBox<AbilityRequirement> searchBox;
    FieldInfo rectField;

    public AbilityPage_RequirementSection() {
        shown = true;
        searchBox = new SearchBox<AbilityRequirement>(null, AddRequirement, "Add Requirement", "Requirements");
        rectField = typeof(EditorGUILayout).GetField("s_LastRect", BindingFlags.Static | BindingFlags.NonPublic);
    }

    private void AddRequirement(Type type) {
        targetItem.InstanceRef.requirements.Add(Activator.CreateInstance(type) as AbilityRequirement);
        targetItem.Rebuild();
    }

    public override void Render() {
        if (serialRoot == null) return;

        shown = EditorGUILayout.Foldout(shown, "Requirements");

        if (shown) {
            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250f;
            EditorGUI.indentLevel += 2;
            for (int i = 0; i < targetItem.InstanceRef.requirements.Count; i++) {
                string propName = "__requirement" + i;
                string type = serialRoot.FindProperty(propName).type;
                if(EditorGUILayout.PropertyField(serialRoot.FindProperty(propName), new GUIContent(type, icon), false)) {
                    SerializedProperty p = serialRoot.FindProperty(propName);
                    DrawerUtil.RenderChildren(p, targetItem.InstanceRef.requirements[i].GetType());
                }
            }
            GUILayout.Space(20f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            searchBox.RenderLayout();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }


}