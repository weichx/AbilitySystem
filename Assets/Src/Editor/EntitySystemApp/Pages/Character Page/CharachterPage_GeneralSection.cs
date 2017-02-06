using UnityEditor;
using UnityEngine;
using EntitySystem;

public class CharacterPage_GeneralSection : SectionBase<Character> {

    public CharacterPage_GeneralSection(float spacing) : base(spacing) {}

    public override void Render() {
        if (rootProperty == null) return;
        SerializedPropertyX isPlayer = rootProperty.FindProperty("isPlayer");
        SerializedPropertyX parameters = rootProperty.FindProperty("parameters");


        GUILayout.BeginVertical();
        isPlayer.Value = EditorGUILayout.Toggle(new GUIContent("Player"), (bool)isPlayer.Value);
        GUILayout.EndVertical();
    }
}