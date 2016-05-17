using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StatusPage_GeneralSection : StatusPage_SectionBase {

    public StatusPage_GeneralSection() : base() { }


    public override void Render() {
        if (serialRoot == null) return;

        SerializedProperty isExpirable = statusProperty.FindPropertyRelative("IsExpirable");
        SerializedProperty isDispellable = statusProperty.FindPropertyRelative("IsDispellable");
        SerializedProperty isRefreshable = statusProperty.FindPropertyRelative("IsRefreshable");
        SerializedProperty isUnique = statusProperty.FindPropertyRelative("IsUnique");
        SerializedProperty duration = statusProperty.FindPropertyRelative("duration").FindPropertyRelative("baseValue");
        SerializedProperty tickRate = statusProperty.FindPropertyRelative("tickRate").FindPropertyRelative("baseValue");
        SerializedProperty ticks = statusProperty.FindPropertyRelative("ticks").FindPropertyRelative("baseValue");

        GUILayout.BeginHorizontal();
        isExpirable.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Is Expirable"), isExpirable.boolValue);
        isDispellable.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Is Dispellable"), isDispellable.boolValue);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        isRefreshable.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Is Refreshable"), isRefreshable.boolValue);
        isUnique.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Is Unique"), isUnique.boolValue);
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);

        using (var h = new EditorGUILayout.HorizontalScope()) {
            duration.floatValue = EditorGUILayout.FloatField("Duration", duration.floatValue);
        }

        using (var h = new EditorGUILayout.HorizontalScope()) {
            ticks.floatValue = EditorGUILayout.FloatField("Ticks", ticks.floatValue);
        }

        using (var h = new EditorGUILayout.HorizontalScope()) {
            tickRate.floatValue = EditorGUILayout.FloatField("Tick Rate", tickRate.floatValue);
        }
    }

}
