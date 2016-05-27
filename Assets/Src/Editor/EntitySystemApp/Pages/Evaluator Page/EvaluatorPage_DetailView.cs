using Intelligence;
using UnityEngine;
using UnityEditor;

public class EvaluatorPage_DetailView {

	private Vector2 scrollPosition;
	private AssetItem<DecisionScoreEvaluator> target;
	private EvaluatorPage_NameSection nameSection;
	private EvaluatorPage_ConsiderationSection considerationSection;
	private EvaluatorPage_RequirementSection requirementSection;
	private EvaluatorPage_GeneralSection generalSection;

	public EvaluatorPage_DetailView() {
		nameSection = new EvaluatorPage_NameSection(); 
		considerationSection = new EvaluatorPage_ConsiderationSection();
		requirementSection = new EvaluatorPage_RequirementSection();
		generalSection = new EvaluatorPage_GeneralSection();
	}

	public void Render() {
		if(target == null || target.SerializedObject == null) return;
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		GUILayout.BeginVertical();
		nameSection.Render();
		GUILayout.Space(5);
		GUILayout.EndVertical();

		GUILayout.BeginVertical();
		generalSection.Render();
		GUILayout.Space(5);
		GUILayout.EndVertical();

		GUILayout.BeginVertical();
		requirementSection.Render();
		GUILayout.Space(5);
		GUILayout.EndVertical();

		GUILayout.BeginVertical();
		considerationSection.Render();
		GUILayout.Space(5);
		GUILayout.EndVertical();

		GUILayout.EndScrollView();
	}

	public void SetTargetObject(AssetItem<DecisionScoreEvaluator> target) {
		this.target = target;
		nameSection.SetTargetObject(target);
		requirementSection.SetTargetObject(target);
		considerationSection.SetTargetObject(target);
		generalSection.SetTargetObject(target);
	}

}