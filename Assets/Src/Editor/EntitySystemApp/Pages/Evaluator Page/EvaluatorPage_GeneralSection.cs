using UnityEngine;
using UnityEditor;
using Intelligence;
using System;

public class EvaluatorPage_GeneralSection : EvaluatorPage_SectionBase {

	private Type[] contextTypes;
	private string[] contextTypeNames;

	public EvaluatorPage_GeneralSection() {
		contextTypes = Reflector.FindSubClasses<Context>(true).ToArray();
		contextTypeNames = new string[contextTypes.Length];
		for(int i = 0; i < contextTypes.Length; i++) {
			contextTypeNames[i] = Util.SplitAndTitlize(contextTypes[i].Name);
		}
	}

	public override void Render() {
		if(targetItem == null) return;

		EditorGUILayout.Popup("Context", 0, contextTypeNames);
	}

}
