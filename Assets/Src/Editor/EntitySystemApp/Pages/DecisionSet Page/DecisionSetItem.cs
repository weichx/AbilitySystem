//using System;
//using UnityEngine;
//using UnityEditor;
//using Intelligence;
//using System.Collections.Generic;
//using System.Text;

//public class DecisionSetItem : AssetItem<DecisionSet> {

//	public List<SerializedObject> serializedDecisions;
//	public List<bool> displayed;

//	public DecisionSetItem(AssetCreator creator) : base(creator) { 
//		serializedDecisions = new List<SerializedObject>();
//		displayed = new List<bool>();
//	}

//	public void AddDecision() {
//		//var decision = new Decision();
//		//decision.name = "Decision " + instanceRef.decisions.Count;
//		//decision.action = new NoOpAction();
//		//decision.dse = new NoOpDSE();
//		//instanceRef.decisions.Add(decision);
//		//serializedDecisions.Add(CreateSerializedObject(decision));
//		//displayed.Add(true);
//	}

//	public void ChangeActionType(int idx, Type actionType) {
//		//Decision decision = instanceRef.decisions[idx];
//		//decision.action = Activator.CreateInstance(actionType) as CharacterAction;
//		////todo -- warn
//		//decision.dse = new NoOpDSE();
//		//serializedDecisions[idx].Dispose();
//		//serializedDecisions[idx] = null;
//		//serializedDecisions[idx] = CreateSerializedObject(decision);
//	}

//	public void ChangeDSEType(int idx, Type dseType) {
//		//Decision decision = instanceRef.decisions[idx];
//		//decision.dse = Activator.CreateInstance(dseType) as DecisionScoreEvaluator;
//		//serializedDecisions[idx].Dispose();
//		//serializedDecisions[idx] = null;
//		//serializedDecisions[idx] = CreateSerializedObject(decision);
//	}

//	public override void Load() {
//		base.Load();
//	    int count = 0;//instanceRef.decisions.Count;
//		//displayed = new List<bool>(instanceRef.decisions.Count);
//		//serializedDecisions = new List<SerializedObject>(instanceRef.decisions.Count);
//		for(int i = 0; i < count; i++) {
//			displayed.Add(false);
//			serializedDecisions.Add(null);
//		}
//	}

//    public bool IsCompiled(int index) {
//        return serializedDecisions[index] != null;
//    }

//    public void Compile(int index) {
//       // serializedDecisions[index] = CreateSerializedObject(instanceRef.decisions[index]);
//    }

//	//protected override void InitializeScriptable() {
//	//	scriptableType.GetField("decisionSet").SetValue(scriptable, instanceRef);
//	//}

//	//private SerializedObject CreateSerializedObject(Decision decision) {
//	//	string code = GenerateDecisionCode(decision);
//	//	Type scriptableType = ScriptableObjectCompiler.CreateScriptableType(code, GetAssemblies(), "GeneratedDecision");
//	//	ScriptableObject scriptable = ScriptableObject.CreateInstance(scriptableType);
//	//	scriptableType.GetField("action").SetValue(scriptable, decision.action);
//	//	scriptableType.GetField("dse").SetValue(scriptable, decision.dse);
//	//	return new SerializedObject(scriptable);
//	//}

//	private string GenerateActionCode(CharacterAction action) {
//		return "public " + action.GetType().Name + " action;";
//	}

//	private string GenerateDSECode(DecisionScoreEvaluator dse) {
//		return "public " + dse.GetType().Name + " dse;";
//	}

//	private string GenerateDecisionCode(Decision decision) {
//		StringBuilder builder = new StringBuilder();
//		builder.AppendLine("using UnityEngine;");
//		builder.AppendLine("using Intelligence;");
//		builder.AppendLine("public class GeneratedDecision : ScriptableObject {");
//		builder.AppendLine("public new string name;");
//		builder.AppendLine("public string description;");
//		builder.AppendLine(GenerateActionCode(decision.action));
//		builder.AppendLine(GenerateDSECode(decision.dse));
//		builder.AppendLine("}");
//		return builder.ToString();
//	}

//	//protected override string GetCodeString() {
//	//	string code = "using UnityEngine;\n";
//	//	code += "public class GeneratedScriptable : ScriptableObject {\n";
//	//	code += "\tpublic DecisionSet decisionSet;\n";
//	//	code += "}";
//	//	return code;
//	//}
//}