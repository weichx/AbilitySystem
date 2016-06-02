using System;
using UnityEngine;
using UnityEditor;
using Intelligence;
using System.Collections.Generic;
using System.Reflection;

public class DecisionSetPage_DecisionList : ListSection<DecisionPackage> {

    public DecisionSetPage_DecisionList(float spacing) : base(spacing, false) { }

    private class DecisionRenderData : RenderData {
        public bool isActionShown;
        public bool isEvaluatorShown;
        public bool isCollectorShown;
        public string[] evaluatorOptions;
        public string[] contextCollectorNames;
        public List<DecisionEvaluatorCreator> evaluatorCreators;
        public List<Type> contextCollectorTypes;
        public int contextCollectorIndex;
        public int evaluatorIndex;
        public EvaluatorPage_ConsiderationSection considerationSection;
        public EvaluatorPage_RequirementSection requirementSection;

        public DecisionRenderData() {
            considerationSection = new EvaluatorPage_ConsiderationSection(0);
            requirementSection = new EvaluatorPage_RequirementSection(0);
        }

        public void GetEvaluatorTemplateData(Type contextType) {
            if (evaluatorCreators != null) return;
            string[] guids = AssetDatabase.FindAssets("t:DecisionEvaluatorCreator");
            evaluatorCreators = new List<DecisionEvaluatorCreator>();
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var creator = AssetDatabase.LoadAssetAtPath<DecisionEvaluatorCreator>(path);
                if (creator != null) {
                    Type evaluatorType = creator.GetContextType();
                    if (evaluatorType == null) {
                        evaluatorType = creator.Create().contextType;
                    }
                    if (evaluatorType.IsAssignableFrom(contextType)) {
                        evaluatorCreators.Add(creator);
                    }
                }
            }

            evaluatorCreators.Insert(0, null);
            evaluatorOptions = new string[evaluatorCreators.Count];
            if (evaluatorCreators.Count == 0) {
                evaluatorOptions = new string[1];
                evaluatorOptions[0] = " No Matching Evaluators Found";
            }
            else {
                evaluatorOptions = new string[evaluatorCreators.Count];
                evaluatorOptions[0] = "-- None --";
            }
            for (int i = 1; i < evaluatorOptions.Length; i++) {
                evaluatorOptions[i] = evaluatorCreators[i].name;
            }
        }

        public void GetContextCollectorData(CharacterAction action, ContextCollector collector) {
            contextCollectorIndex = 0;
            if (collector != null) {
                Type collectorType = collector.GetType();
                for (int i = 1; i < contextCollectorTypes.Count; i++) {
                    if (collectorType == contextCollectorTypes[i]) {
                        contextCollectorIndex = i;
                        break;
                    }
                }
            }
            if (contextCollectorTypes != null) return;
            contextCollectorTypes = Reflector.FindSubClasses(typeof(ContextCollector<>));
            var genType = typeof(ContextCollector<>);
            var genericType = genType.MakeGenericType(new Type[] { action.ContextType });

            contextCollectorTypes = contextCollectorTypes.FindAll((type) => {
                return genericType.IsAssignableFrom(type);
            });
            contextCollectorTypes.Insert(0, null);
            if (contextCollectorTypes.Count == 0) {
                contextCollectorNames = new string[1];
                contextCollectorNames[0] = " No Matching Collector Types Found";
            }
            else {
                contextCollectorNames = new string[contextCollectorTypes.Count];
                contextCollectorNames[0] = "-- None --";
            }
            for (int i = 1; i < contextCollectorNames.Length; i++) {
                contextCollectorNames[i] = contextCollectorTypes[i].Name;
            }
        }
    }



    protected override string ListRootName {
        get { return "decisions"; }
    }

    protected override string GetItemFoldoutLabel(SerializedPropertyX property, RenderData data) {
        if (!data.isDisplayed) {
            return property["name"].GetValue<string>();
        }
        else {
            return "";
        }
    }

    protected override RenderData CreateDataInstance(SerializedPropertyX property, bool isNewTarget) {
        var data = new DecisionRenderData();
        data.isDisplayed = !isNewTarget;
        data.isActionShown = false;
        data.considerationSection.SetTargetProperty(property["evaluator"]);
        data.requirementSection.SetTargetProperty(property["evaluator"]);
        return data;
    }

    protected override void AddListItem(Type type) {
        listRoot.ArraySize++;
        SerializedPropertyX newChild = listRoot.GetChildAt(listRoot.ArraySize - 1);
        var decision = new Decision();
        decision.action = Activator.CreateInstance(type) as CharacterAction;
        decision.name = decision.action.GetType().Name;
        decision.evaluator = new DecisionEvaluator(decision.action.ContextType);
        newChild.Value = decision;
        DecisionRenderData data = CreateDataInstance(newChild, true) as DecisionRenderData;
        renderData.Add(data);
    }

    protected override void RenderBody(SerializedPropertyX property, RenderData renderData, int index) {
        DecisionRenderData data = renderData as DecisionRenderData;

        EditorGUI.indentLevel++;

        EditorGUILayoutX.PropertyField(property["name"]);
        EditorGUILayoutX.PropertyField(property["description"]);
        EditorGUILayoutX.PropertyField(property["weight"]);

        SerializedPropertyX action = property["action"];
        SerializedPropertyX collector = property["contextCollector"];

        CharacterAction charAction = action.GetValue<CharacterAction>();
        string contextString = "<" + charAction.ContextType.Name + ">";
        string actionString = "Action (" + action.Type.Name + contextString + ")";
        if (action.ChildCount > 0) {
            data.isActionShown = EditorGUILayout.Foldout(data.isActionShown, actionString);
        }
        else {
            GUIContent content = new GUIContent(actionString);
            content.image = EditorGUIUtility.FindTexture("cs Script Icon");
            EditorGUILayout.LabelField(content);
            data.isActionShown = false;
        }
        if (data.isActionShown) {
            EditorGUI.indentLevel++;
            EditorGUILayoutX.DrawProperties(action);
            EditorGUI.indentLevel--;
        }

        data.GetContextCollectorData(charAction, collector.GetValue<ContextCollector>());
        string[] options = data.contextCollectorNames;
        int currentIndex = data.contextCollectorIndex;
        int idx = EditorGUILayout.Popup("Collector", currentIndex, options, GUILayout.Width(EditorGUIUtility.labelWidth + 300f));
        if (idx != data.contextCollectorIndex) {
            if (idx == 0) {
                collector.Value = null; //todo return an empty collector type?
            }
            else {
                Type newCollectorType = data.contextCollectorTypes[idx];
                collector.Value = Activator.CreateInstance(newCollectorType) as ContextCollector;
            }
        }
        if (collector.Value != null) {
            EditorGUI.indentLevel++;
            EditorGUILayoutX.DrawProperties(collector);
            EditorGUI.indentLevel--;
        }

        data.isEvaluatorShown = EditorGUILayout.Foldout(data.isEvaluatorShown, "Evaluator");
        if (data.isEvaluatorShown) {
            EditorGUI.indentLevel++;
            SerializedPropertyX evaluator = property["evaluator"];
            SerializedPropertyX considerations = evaluator["considerations"];
            SerializedPropertyX requirements = evaluator["requirements"];
            if (considerations.ChildCount == 0 && requirements.ChildCount == 0) {
                data.GetEvaluatorTemplateData(charAction.ContextType);
                int newIdx = EditorGUILayout.Popup("Set From Template", 0, data.evaluatorOptions, GUILayout.Width(EditorGUIUtility.labelWidth + 300f));
                if (newIdx != 0) {
                    evaluator.Value = data.evaluatorCreators[newIdx].Create();
                    data.requirementSection.SetTargetProperty(evaluator);
                    data.considerationSection.SetTargetProperty(evaluator);
                }
            }
            data.requirementSection.Render();
            data.considerationSection.Render();
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;

    }

    protected override SearchBox CreateSearchBox() {
        Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
        return new SearchBox(icon, typeof(CharacterAction<>), AddListItem, "Add Action", "Actions");
    }
}