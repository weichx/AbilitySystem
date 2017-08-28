using UnityEditor;
using UnityEngine;
using EntitySystem;
using System.Collections.Generic;

public class InventoryItemPage_GeneralSection : SectionBase<InventoryItem> {

    public InventoryItemPage_GeneralSection(float spacing) : base(spacing) {}

    private HashSet<ItemType> _itemTypesCheck = new HashSet<ItemType>(); // workaround for the fact that we cannot make serializable hashsets

    public override void Render() {
        if (rootProperty == null) return;
        SerializedPropertyX isSoulbound = rootProperty.FindProperty("isSoulbound");
        SerializedPropertyX isUnique = rootProperty.FindProperty("isUnique");
        SerializedPropertyX isStackable = rootProperty.FindProperty("isStackable");
        SerializedPropertyX isUsable = rootProperty.FindProperty("isUsable");
        SerializedPropertyX isDestructable = rootProperty.FindProperty("isDestructable");
        SerializedPropertyX isTypes = rootProperty.FindProperty("isType");

        GUILayout.BeginVertical();
        isSoulbound.Value = EditorGUILayout.Toggle(new GUIContent("Soulbound"), (bool)isSoulbound.Value);
        isUnique.Value = EditorGUILayout.Toggle(new GUIContent("Unique"), (bool)isUnique.Value);
        isUsable.Value = EditorGUILayout.Toggle(new GUIContent("Usable"), (bool)isUsable.Value);
        isDestructable.Value = EditorGUILayout.Toggle(new GUIContent("Destructable"), (bool)isDestructable.Value);
        isStackable.Value = EditorGUILayout.Toggle(new GUIContent("Stackable"), (bool)isStackable.Value);

        EditorGUILayout.LabelField("Attributes (" + isTypes.ArraySize + ")");
        if (GUILayout.Button("+", GUILayout.Width(20f), GUILayout.Height(15f))) {
            // part of the workaround
            if(isTypes.ArraySize > 0) {
                if(!_itemTypesCheck.Add((ItemType)isTypes.GetChildAt(isTypes.ArraySize - 1).Value)) {
                    return;
                }
            }
            isTypes.ArraySize++;
        }
        EditorGUI.indentLevel++;
        for (int i = 0; i < isTypes.ArraySize; i++) {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(20f), GUILayout.Height(15f))) {
                _itemTypesCheck.Remove((ItemType)isTypes.GetChildAt(i).Value); // also part of the workaround
                isTypes.DeleteArrayElementAt(i);
            }
            EditorGUILayoutX.PropertyField(isTypes.GetChildAt(i));
            GUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
        GUILayout.EndVertical();
    }
}