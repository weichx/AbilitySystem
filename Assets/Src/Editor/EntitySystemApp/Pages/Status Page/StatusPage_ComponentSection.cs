using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StatusPage_ComponentSection : StatusPage_SectionBase {

    private bool shown;
    private SearchBox<StatusEffectComponent> searchBox;

    public StatusPage_ComponentSection() {
        shown = true;
        searchBox = new SearchBox<StatusEffectComponent>(null, AddComponent, "Add Status Component", "Status Components");
    }

    private void AddComponent(Type type) {
        targetItem.InstanceRef.components.Add(Activator.CreateInstance(type) as StatusEffectComponent);
        targetItem.Rebuild();
    }

    public void RenderChildren(SerializedProperty p, Type componentType) {
        EditorGUI.indentLevel += 2;
        while (p.NextVisible(true)) {
            if (p.name.StartsWith("__component")) {
                break;
            }
            DrawerUtil.DrawProperty(p, componentType);
        }
        EditorGUI.indentLevel -= 2;
    }

    public override void Render() {
        if (serialRoot == null) return;
        shown = EditorGUILayout.Foldout(shown, "Components");
        if (shown) {
            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250f;
            EditorGUI.indentLevel += 2;
            for (int i = 0; i < targetItem.InstanceRef.components.Count; i++) {
                string propName = "component" + i;
                string type = serialRoot.FindProperty(propName).type;
                if (EditorGUILayout.PropertyField(serialRoot.FindProperty(propName), new GUIContent(type, icon), false)) {
                    SerializedProperty p = serialRoot.FindProperty(propName);
                    RenderChildren(p, targetItem.InstanceRef.components[i].GetType());
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