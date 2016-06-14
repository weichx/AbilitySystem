using System;
using UnityEditor;
using UnityEngine;

[PropertyDrawerFor(typeof(SkillBookEntry))]
public class SkillBookEntryDrawer : PropertyDrawerX {

    private TypePopup popup;
    private bool shown;

    public SkillBookEntryDrawer() {
        popup = new TypePopup(typeof(PlayerContextCreator), true, Validate);
    }

    private bool Validate(Type type) {
        //return context type matches ability's
        return true;// type.IsAssignableFrom();
    }

    public override void OnGUI(SerializedPropertyX property, GUIContent label) {
        shown = EditorGUILayout.Foldout(shown, label);
        if (!shown) return;

        EditorGUI.indentLevel++;
        EditorGUILayoutX.PropertyField(property["abilityCreator"]);
        if (property["abilityCreator"].Changed) {
            //todo assert type matches
        }
        if (property["abilityCreator"].Value == null) {
            property["contextCreator"].Value = null;
            return;
        }

        Type type;
        PlayerContextCreator current = property["contextCreator"].GetValue<PlayerContextCreator>();
        Type currentType = null;
        if (current != null)  currentType = current.GetType();
        SerializedPropertyX creatorProperty = property["contextCreator"];

        if (popup.DrawPopup("Context Constructor", currentType, out type)) {
            if (type != null) {
                creatorProperty.Value = Activator.CreateInstance(type);
            }
            else {
                creatorProperty.Value = null;
            }
        }

        if (creatorProperty.Value != null) {
            EditorGUI.indentLevel++;
            EditorGUILayoutX.DrawProperties(creatorProperty);
            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
    }
}