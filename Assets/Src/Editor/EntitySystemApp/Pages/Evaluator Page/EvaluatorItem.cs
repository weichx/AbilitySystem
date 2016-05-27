using Intelligence;

public class EvaluatorItem : AssetItem<DecisionScoreEvaluator> {

	public EvaluatorItem(AssetCreator creator) : base(creator) { }

	protected override void InitializeScriptable() {
		scriptableType.GetField("dse").SetValue(scriptable, instanceRef);
		for (int i = 0; i < instanceRef.considerations.Count; i++) {
			scriptableType.GetField("__consideration" + i).SetValue(scriptable, instanceRef.considerations[i]);
		}
		for (int i = 0; i < instanceRef.requirements.Count; i++) {
			scriptableType.GetField("__requirement" + i).SetValue(scriptable, instanceRef.requirements[i]);
		}
	}

	protected override string GetCodeString() {
		string code = "using Intelligence;";
		code += "using UnityEngine;";
		code += "public class GeneratedScriptable : ScriptableObject {";
		code += "public DecisionScoreEvaluator dse;";
		for(int i = 0; i < instanceRef.considerations.Count; i++) {
			code += "public " + instanceRef.considerations[i].GetType().Name + " __consideration" + i + ";";
		}
		for(int i = 0; i < instanceRef.requirements.Count; i++) {
			code += "public " + instanceRef.requirements[i].GetType().Name + " __requirement" + i + ";";
		}
		code += "}";
		return code;
	}
}