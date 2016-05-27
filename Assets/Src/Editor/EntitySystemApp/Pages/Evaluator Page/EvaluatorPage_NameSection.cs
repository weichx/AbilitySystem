using UnityEngine;
using UnityEditor;

public class EvaluatorPage_NameSection : EvaluatorPage_SectionBase {

	public override void Render() {
		SerializedProperty nameProp = rootProperty.FindPropertyRelative("id");
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		GUILayout.Space(20f);
		float labelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 100f;
		EditorGUILayout.PropertyField(nameProp, new GUIContent("Evaluator Name"));
		EditorGUIUtility.labelWidth = labelWidth;
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