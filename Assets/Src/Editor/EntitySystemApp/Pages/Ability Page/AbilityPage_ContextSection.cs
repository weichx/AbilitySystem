using System;
using UnityEditor;
using UnityEngine;
using EntitySystem;
using Intelligence;
using System.Collections.Generic;

public class AbilityPage_ContextSection : SectionBase<Ability> {

	private List<Type> contextTypeList;
	private string[] contextTypeNames;
    private TypePopup typePopUp;

	public AbilityPage_ContextSection(float spacing) : base(spacing) {
        typePopUp = new TypePopup(typeof(Context), false);
	}

	public override void Render() {
		if (targetItem == null) return;
	    SerializedPropertyX contextTypeProp = rootProperty["contextType"];
	    Type selectedType;
	    if (typePopUp.DrawPopup("Context Type", contextTypeProp.GetValue<Type>(), out selectedType)) {
            SerializedPropertyX componentsProp = rootProperty["components"];
            SerializedPropertyX requirementsProp = rootProperty["requirements"];
            List<SerializedPropertyX> nukeList = new List<SerializedPropertyX>();
            for (int i = 0; i < componentsProp.ChildCount; i++) {
                AbilityComponent component = componentsProp.GetChildAt(i).Value as AbilityComponent;
                if (!selectedType.IsAssignableFrom(component.GetContextType())) {
                    nukeList.Add(componentsProp.GetChildAt(i));
                }
            }
            for (int i = 0; i < requirementsProp.ChildCount; i++) {
                Requirement requirement = requirementsProp.GetChildAt(i).Value as Requirement;
                if (!selectedType.IsAssignableFrom(requirement.GetContextType())) {
                    nukeList.Add(requirementsProp.GetChildAt(i));
                }
            }

            if (nukeList.Count > 0) {
                if (ShouldNuke(nukeList, selectedType)) {
                    for (int i = 0; i < nukeList.Count; i++) {
                        SerializedPropertyX toNuke = nukeList[i];
                        int reqChildIndex = requirementsProp.GetChildIndex(toNuke);
                        int comChildIndex = componentsProp.GetChildIndex(toNuke);
                        requirementsProp.DeleteArrayElementAt(reqChildIndex);
                        componentsProp.DeleteArrayElementAt(comChildIndex);
                    }
                    contextTypeProp.Value = selectedType;
                }
            }
            else {
                contextTypeProp.Value = selectedType;
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