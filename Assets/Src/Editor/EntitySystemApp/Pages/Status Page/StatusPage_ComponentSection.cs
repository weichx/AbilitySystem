using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StatusPage_ComponentSection : StatusPage_SectionBase {

    private bool shown;
    private SearchBox<StatusEffectComponent> searchBox;

    public StatusPage_ComponentSection(StatusPage page) : base(page) {
        shown = true;
        searchBox = new SearchBox<StatusEffectComponent>(null, AddComponent, "Add Status Component", "Status Components");
    }

    private void AddComponent(Type type) {
        page.activeEntry.statusEffect.components.Add(Activator.CreateInstance(type) as StatusEffectComponent);
        page.Compile();
    }

    public override void Render() {
        if (target == null) return;
        shown = EditorGUILayout.Foldout(shown, "Components");
        if (shown) {
            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250f;
            EditorGUI.indentLevel += 2;
            for (int i = 0; i < page.activeEntry.statusEffect.components.Count; i++) {
                string type = target.FindProperty("component" + i).type;
                EditorGUILayout.PropertyField(target.FindProperty("component" + i), new GUIContent(type, icon), true);
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