using Intelligence;
using UnityEngine;
using UnityEditor;

public class DecisionPackage_NameSection : SectionBase<DecisionPackage> {

    public DecisionPackage_NameSection(float spacing) : base(spacing) {}
    
    public override void Render() {
        if (rootProperty == null) return;
        SerializedPropertyX nameProp = rootProperty["id"];
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Space(20f);
        EditorGUILayoutX.PropertyField(nameProp, new GUIContent("Decision Package Name"), true);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Restore")) {
            targetItem.Restore();
            GUIUtility.keyboardControl = 0;
        }
        if (GUILayout.Button("Delete")) {
            targetItem.QueueDelete();
        }
        if (GUILayout.Button("Save")) {
            targetItem.Save();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
}
