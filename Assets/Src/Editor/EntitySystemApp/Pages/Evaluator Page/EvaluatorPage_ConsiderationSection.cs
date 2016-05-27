using UnityEngine;
using UnityEditor;
using Intelligence;

public class EvaluatorPage_ConsiderationSection : EvaluatorPage_SectionBase {

	private bool shown;

	public EvaluatorPage_ConsiderationSection() {
		shown = true;
	}

	public override void Render() {

		shown = EditorGUILayout.Foldout(shown, "Considerations");

		if(shown && targetItem != null) {
			
		}

	}

}
