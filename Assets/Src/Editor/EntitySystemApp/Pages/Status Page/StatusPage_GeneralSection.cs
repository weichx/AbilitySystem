using UnityEngine;
using UnityEditor;

public class StatusPage_GeneralSection : SectionBase<StatusEffect> {

    public StatusPage_GeneralSection(float spacing) : base(spacing) { }

    public override void Render() {
        if (rootProperty == null) return;
        SerializedPropertyX isExpirable = rootProperty.FindProperty("IsExpirable");
        SerializedPropertyX isDispellable = rootProperty.FindProperty("IsDispellable");
        SerializedPropertyX isRefreshable = rootProperty.FindProperty("IsRefreshable");
        SerializedPropertyX isUnique = rootProperty.FindProperty("IsUnique");
        SerializedPropertyX duration = rootProperty.FindProperty("duration").FindProperty("baseValue");
        SerializedPropertyX tickRate = rootProperty.FindProperty("tickRate").FindProperty("baseValue");
        SerializedPropertyX ticks = rootProperty.FindProperty("ticks").FindProperty("baseValue");

        GUILayout.BeginHorizontal();
        isExpirable.Value = EditorGUILayout.ToggleLeft(new GUIContent("Is Expirable"), (bool)isExpirable.Value);
        isDispellable.Value = EditorGUILayout.ToggleLeft(new GUIContent("Is Dispellable"), (bool)isDispellable.Value);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        isRefreshable.Value = EditorGUILayout.ToggleLeft(new GUIContent("Is Refreshable"), (bool)isRefreshable.Value);
        isUnique.Value = EditorGUILayout.ToggleLeft(new GUIContent("Is Unique"), (bool)isUnique.Value);
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);

        //using (var h = new EditorGUILayout.HorizontalScope()) {
        //    duration.Value = EditorGUILayout.FloatField("Duration", duration.GetValue<float>());
        //}

        //using (var h = new EditorGUILayout.HorizontalScope()) {
        //    ticks.Value = EditorGUILayout.FloatField("Ticks", ticks.GetValue<float>());
        //}

        //using (var h = new EditorGUILayout.HorizontalScope()) {
        //    tickRate.Value = EditorGUILayout.FloatField("Tick Rate", tickRate.GetValue<float>());
        //}
    }

}
