using UnityEditor;
using UnityEngine;
using EntitySystem;

public class CharacterPage_GeneralSection : SectionBase<Character> {

    public CharacterPage_GeneralSection(float spacing) : base(spacing) {}

    public override void Render() {
        if (rootProperty == null) return;
        SerializedPropertyX isPlayer = rootProperty.FindProperty("isPlayer");
        SerializedPropertyX items = rootProperty.FindProperty("items");
        SerializedPropertyX abilities = rootProperty.FindProperty("abilities");
        SerializedPropertyX prefab = rootProperty.FindProperty("prefab");

        GUILayout.BeginVertical();
        isPlayer.Value = EditorGUILayout.Toggle(new GUIContent("Player"), (bool)isPlayer.Value);
        EditorGUILayoutX.PropertyField(prefab);

        EditorGUILayout.LabelField("Items (" + items.ArraySize + ")");
        if (GUILayout.Button("+", GUILayout.Width(20f), GUILayout.Height(15f))) {
            items.ArraySize++;
        }
        EditorGUI.indentLevel++;
        for (int i = 0; i < items.ArraySize; i++) {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(20f), GUILayout.Height(15f))) {
                items.DeleteArrayElementAt(i);
            }
            EditorGUILayoutX.PropertyField(items.GetChildAt(i));
            GUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.LabelField("Abilities (" + abilities.ArraySize + ")");
        if (GUILayout.Button("+", GUILayout.Width(20f), GUILayout.Height(15f))) {
            abilities.ArraySize++;
        }
        EditorGUI.indentLevel++;
        for (int i = 0; i < abilities.ArraySize; i++) {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(20f), GUILayout.Height(15f))) {
                abilities.DeleteArrayElementAt(i);
            }
            EditorGUILayoutX.PropertyField(abilities.GetChildAt(i));
            GUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;

        GUILayout.EndVertical();
    }
}