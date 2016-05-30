using UnityEngine;
using Intelligence;
using System;

public class EvaluatorPage_GeneralSection : SectionBase<DecisionScoreEvaluator> {
	private Type[] contextTypes;
	private string[] contextTypeNames;
    public SerializedObjectX root;

	public EvaluatorPage_GeneralSection(float spacing) : base(spacing) {
        contextTypes = Reflector.FindSubClasses<Context>(true).ToArray();
		contextTypeNames = new string[contextTypes.Length];
		for(int i = 0; i < contextTypes.Length; i++) {
			contextTypeNames[i] = Util.SplitAndTitlize(contextTypes[i].Name);
		}
	}

	public override void Render() {

	}

}
