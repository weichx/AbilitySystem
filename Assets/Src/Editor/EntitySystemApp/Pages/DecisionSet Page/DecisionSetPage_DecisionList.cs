using System;
using UnityEngine;
using UnityEditor;
using Intelligence;
using System.Collections.Generic;
using System.Reflection;

public class DecisionSetPage_DecisionList : DecisionSetPage_SectionBase {

	private Type[] actionTypes;
	private string[] actionNames;
	private Type[] dseTypes;
	private string[] dseNames;
	public DecisionSetItem item;

	public DecisionSetPage_DecisionList() {
		actionTypes = Reflector.FindSubClasses<CharacterAction>().ToArray();
		actionNames = new string[actionTypes.Length];
		for(int i = 0; i < actionTypes.Length; i++) {
			actionNames[i] = Util.SplitAndTitlize(actionTypes[i].Name);
		}
		dseTypes = Reflector.FindSubClasses<DecisionScoreEvaluator>().ToArray();
		dseNames = new string[dseTypes.Length];
		for(int i = 0; i < dseTypes.Length; i++) {
			dseNames[i] = Util.SplitAndTitlize(dseTypes[i].Name);
		}
	}

	public override void SetTargetObject(AssetItem<DecisionSet> targetItem) {
		base.SetTargetObject(targetItem);
		item = targetItem as DecisionSetItem;
		//todo -- recreate serialized objects and array
	}

	public void Render() {
		if(serialRoot == null) return;

		for(int i = 0; i < item.InstanceRef.decisions.Count; i++) {
			EditorGUILayout.BeginVertical();
			item.displayed[i] = RenderDecision(item.InstanceRef.decisions[i], item.displayed[i]);
		    if (item.serializedDecisions[i] != null) {
		        item.serializedDecisions[i].ApplyModifiedProperties();
		    }
		    GUILayout.Space(10);
			EditorGUILayout.EndVertical();
		}
			
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Add Decision", GUILayout.Width(250f))) {
			item.AddDecision();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private bool RenderDecision(Decision decision, bool isDisplayed) {
		
		EditorGUILayout.BeginHorizontal();

		if(isDisplayed) {
			isDisplayed = EditorGUILayout.Foldout(isDisplayed, "", new GUIStyle(EditorStyles.foldout) { fixedWidth = 15f });
			decision.name = EditorGUILayout.TextField(decision.name);
		}
        else {
			isDisplayed = EditorGUILayout.Foldout(isDisplayed, decision.name);
		}

        int decisionIndex = item.InstanceRef.decisions.IndexOf(decision);

        if (isDisplayed && !item.IsCompiled(decisionIndex)) {
	        item.Compile(decisionIndex);
	    }

		EditorGUILayout.EndHorizontal();

		if(!isDisplayed) return isDisplayed;

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel += 1;

		decision.description = EditorGUILayout.TextField(new GUIContent("Description"), decision.description);

		RenderAction(decision);
		RenderDSE(decision);
		EditorGUI.indentLevel = indent;

		return isDisplayed;
	}

	private void RenderDSE(Decision decision) {
		int idx = -1;
		int decisionIndex = item.InstanceRef.decisions.IndexOf(decision);

		Type currentDSEType = decision.dse.GetType();
		for(int i = 0; i < dseTypes.Length; i++) {
			if(dseTypes[i] == currentDSEType) {
				idx = i; 
				break;
			}
		}

		if(idx == -1) {
			idx = 0;
			item.ChangeDSEType(decisionIndex, typeof(NoOpDSE));
		}

		int newIdx = EditorGUILayout.Popup("Evaluator", idx, dseNames);
		if(newIdx != idx) {
			item.ChangeDSEType(decisionIndex, dseTypes[newIdx]);
		}

		var serial = item.serializedDecisions[decisionIndex];
		SerializedProperty dseProp = serial.FindProperty("dse");
		if(dseProp == null) {
			Debug.LogError("Can't find dse, make sure " + decision.dse.GetType().Name + " is marked as serializable");
			return;
		}

		EditorGUILayout.BeginVertical();
		DrawerUtil.RenderChildren(dseProp, decision.action.GetType(), 1);
		EditorGUILayout.EndVertical();
	}

	private void RenderAction(Decision decision) {

		int idx = -1;
		int decisionIndex = item.InstanceRef.decisions.IndexOf(decision);

		Type currentActionType = decision.action.GetType();
		for(int i = 0; i < actionTypes.Length; i++) {
			if(actionTypes[i] == currentActionType) {
				idx = i; 
				break;
			}
		}

		if(idx == -1) {
			idx = 0;
			(targetItem as DecisionSetItem).ChangeActionType(decisionIndex, typeof(NoOpAction));
		}

		int newIdx = EditorGUILayout.Popup("Action", idx, actionNames);
		if(newIdx != idx) {
			(targetItem as DecisionSetItem).ChangeActionType(decisionIndex, actionTypes[newIdx]);
		}

		var serial = item.serializedDecisions[decisionIndex];
		SerializedProperty actionProp = serial.FindProperty("action");
		if(actionProp == null) {
			Debug.LogError("Can't find action, make sure " + decision.action.GetType().Name + " is marked as serializable");
			return;
		}

		EditorGUILayout.BeginVertical();
		DrawerUtil.RenderChildren(actionProp, decision.action.GetType(), 1);
		EditorGUILayout.EndVertical();
	}

}