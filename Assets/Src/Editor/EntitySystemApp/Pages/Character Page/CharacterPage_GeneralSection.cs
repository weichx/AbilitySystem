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

        GUILayout.BeginVertical();
        isPlayer.Value = EditorGUILayout.Toggle(new GUIContent("Player"), (bool)isPlayer.Value);

        EditorGUILayout.LabelField("Items (" + items.ArraySize + ")");
        if (GUILayout.Button("+", GUILayout.Width(25f))) {
            items.ArraySize++;
        }
        EditorGUI.indentLevel++;
        for (int i = 0; i < items.ArraySize; i++) {
            EditorGUILayoutX.PropertyField(items.GetChildAt(i));
            if (GUILayout.Button("-", GUILayout.Width(25f), GUILayout.Height(15f))) {
                items.DeleteArrayElementAt(i);
            }
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.LabelField("Abilities (" + abilities.ArraySize + ")");
        if (GUILayout.Button("+", GUILayout.Width(25f))) {
            abilities.ArraySize++;
        }
        EditorGUI.indentLevel++;
        for (int i = 0; i < abilities.ArraySize; i++) {
            EditorGUILayoutX.PropertyField(abilities.GetChildAt(i));
            if (GUILayout.Button("-", GUILayout.Width(25f), GUILayout.Height(15f))) {
                abilities.DeleteArrayElementAt(i);
            }
        }
        EditorGUI.indentLevel--;

        GUILayout.EndVertical();
    }
}