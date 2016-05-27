using UnityEditor;
using Intelligence;

public abstract class EvaluatorPage_SectionBase : SectionBase<DecisionScoreEvaluator> {

	protected override string RootPropertyName {
		get {
			return "dse";
		}
	}
}
