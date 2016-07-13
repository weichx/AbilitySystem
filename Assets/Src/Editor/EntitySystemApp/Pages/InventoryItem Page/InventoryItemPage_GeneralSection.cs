using UnityEditor;
using UnityEngine;

public class InventoryItemPage_GeneralSection : SectionBase<InventoryItem> {

    public InventoryItemPage_GeneralSection(float spacing) : base(spacing) {}

    public override void Render() {
        if (rootProperty == null) return;
        SerializedPropertyX isSoulbound = rootProperty.FindProperty("isSoulbound");
        SerializedPropertyX isUnique = rootProperty.FindProperty("isUnique");
        SerializedPropertyX isStackable = rootProperty.FindProperty("isStackable");
        SerializedPropertyX isUsable = rootProperty.FindProperty("isUsable");
        SerializedPropertyX isDestructable = rootProperty.FindProperty("isDestructable");

        GUILayout.BeginVertical();
        isSoulbound.Value = EditorGUILayout.Toggle(new GUIContent("Soulbound"), (bool)isSoulbound.Value);
        isUnique.Value = EditorGUILayout.Toggle(new GUIContent("Unique"), (bool)isUnique.Value);
        isUsable.Value = EditorGUILayout.Toggle(new GUIContent("Usable"), (bool)isUsable.Value);
        isDestructable.Value = EditorGUILayout.Toggle(new GUIContent("Destructable"), (bool)isDestructable.Value);
        isStackable.Value = EditorGUILayout.Toggle(new GUIContent("Stackable"), (bool)isStackable.Value);
        GUILayout.EndVertical();
    }
}