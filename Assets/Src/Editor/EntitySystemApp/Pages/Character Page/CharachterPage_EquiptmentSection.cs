using EntitySystem;
using UnityEngine;
using UnityEditor;
using System;

public class CharacterPage_EquipmentSection : RandomGeneratorSection<Character> {

    public CharacterPage_EquipmentSection(float spacing) : base(spacing) { }
    private Type lastContextType;

    protected override string FoldOutLabel {
        get { return "Equipment"; }
    }

    protected override string ListRootName {
        get { return "equipment"; }
    }

    public override void CreateRollButton() {
        if (GUILayout.Button("Roll Random")) {

        }
    }

    protected override void RenderBody(SerializedPropertyX property, RenderData data, int index) {
        EditorGUI.indentLevel++;
        EditorGUILayoutX.PropertyField(property, property.label, property.isExpanded);
        EditorGUI.indentLevel--;
    }

    public override void Render() {
        base.Render();
    }
}
