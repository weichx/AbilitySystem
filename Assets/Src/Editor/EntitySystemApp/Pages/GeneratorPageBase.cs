using EntitySystem;
using UnityEngine;
using UnityEditor;
using System;

public abstract class RandomGeneratorSection<T> : ListSection<T> where T : EntitySystemBase, new() {

    public RandomGeneratorSection(float spacing) : base(spacing) {
        shown = false;
    }

    public virtual void CreateRollButton() { }

    protected override void RenderHeader(SerializedPropertyX property, RenderData data, int index) {
        EditorGUILayout.BeginHorizontal();
        data.isDisplayed = true;
        GUILayout.FlexibleSpace();

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    public override void Render() {
        if (rootProperty == null) return;
        EditorGUILayout.BeginVertical();

        if (useFoldout) {
            shown = EditorGUILayout.Foldout(shown, FoldOutLabel);
        }
        else {
            shown = true;
        }
        if (shown) {
            EditorGUI.indentLevel++;
            for (int i = 0; i < listRoot.ChildCount; i++) {
                SerializedPropertyX child = listRoot.GetChildAt(i);
                RenderListItem(child, i);
            }
            CreateRollButton();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

}

