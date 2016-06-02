using UnityEngine;
using UnityEditor;
using Intelligence;
using System.Collections.Generic;
using System;

public class EvaluatorPage_GeneralSection : SectionBase<DecisionEvaluator> {
    private Type[] contextTypes;
    private string[] contextTypeNames;
    public SerializedObjectX root;

    public EvaluatorPage_GeneralSection(float spacing) : base(spacing) {
        contextTypes = Reflector.FindSubClasses<Context>(true).ToArray();
        contextTypeNames = new string[contextTypes.Length];
        for (int i = 0; i < contextTypes.Length; i++) {
            contextTypeNames[i] = Util.SplitAndTitlize(contextTypes[i].Name);
        }
    }

    public override void Render() {
        if (rootProperty == null) return;
        SerializedPropertyX contextTypeProp = rootProperty["contextType"];
        Type currentType = contextTypeProp.GetValue<Type>();
        if (currentType == null) currentType = typeof(Context);
        int idx = 0;

        for (int i = 1; i < contextTypes.Length; i++) {
            if (currentType == contextTypes[i]) {
                idx = i;
                break;
            }
        }

        int newIdx = EditorGUILayout.Popup("Context Type", idx, contextTypeNames, GUILayout.Width(EditorGUIUtility.labelWidth + 300));
        if (idx != newIdx || currentType == null) {
            Type newType = contextTypes[newIdx];
            SerializedPropertyX considerationsProp = rootProperty["considerations"];
            SerializedPropertyX requirementsProp = rootProperty["requirements"];
            List<SerializedPropertyX> nukeList = new List<SerializedPropertyX>();
            for (int i = 0; i < considerationsProp.ChildCount; i++) {
                Consideration consideration = considerationsProp.GetChildAt(i).Value as Consideration;
                if (!newType.IsAssignableFrom(consideration.GetContextType())) {
                    nukeList.Add(considerationsProp.GetChildAt(i));
                }
            }
            for (int i = 0; i < requirementsProp.ChildCount; i++) {
                Requirement requirement = requirementsProp.GetChildAt(i).Value as Requirement;

                if (!newType.IsAssignableFrom(requirement.GetContextType())) {
                    nukeList.Add(requirementsProp.GetChildAt(i));
                }
            }

            if (nukeList.Count > 0) {
                if (ShouldNuke(nukeList, newType)) {
                    for (int i = 0; i < nukeList.Count; i++) {
                        SerializedPropertyX toNuke = nukeList[i];
                        int reqChildIndex = requirementsProp.GetChildIndex(toNuke);
                        int conChildIndex = considerationsProp.GetChildIndex(toNuke);
                        requirementsProp.DeleteArrayElementAt(reqChildIndex);
                        considerationsProp.DeleteArrayElementAt(conChildIndex);
                    }
                    contextTypeProp.Value = newType;
                }
            }
            else {
                contextTypeProp.Value = newType;
            }
        }
    }

    private bool ShouldNuke(List<SerializedPropertyX> nukeList, Type currentType) {
        string message = "The chosen context type (" + currentType.Name + ") is incompatable";
        message += " with the following: \n\n";
        for (int i = 0; i < nukeList.Count; i++) {
            message += nukeList[i].Type + "\n";
        }
        message += "\nRemove them?";
        return EditorUtility.DisplayDialog("Incompatable Components", message, "Yep, Nuke 'em", "No, I changed my mind");

    }
}
