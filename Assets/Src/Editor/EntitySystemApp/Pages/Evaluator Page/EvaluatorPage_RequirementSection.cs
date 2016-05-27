using UnityEngine;
using UnityEditor;
using Intelligence;

public class EvaluatorPage_RequirementSection : EvaluatorPage_SectionBase {

	private bool shown;

	public EvaluatorPage_RequirementSection() {
		shown = true;
	}

	public override void Render() {

		shown = EditorGUILayout.Foldout(shown, "Requirements");

		if(shown && targetItem != null) {

		}

	}

}
