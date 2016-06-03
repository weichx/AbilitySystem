using System;
using UnityEditor;
using UnityEngine;
using Intelligence;
using System.Collections.Generic;

public class AbilityPage_ContextSection : SectionBase<Ability> {

	private List<Type> contextTypeList;
	private string[] contextTypeNames;
    
	public AbilityPage_ContextSection(float spacing) : base(spacing) {
		contextTypeList = Reflector.FindSubClasses<Context>(true);
		contextTypeNames = new string[contextTypeList.Count];
		for(int i = 0; i < contextTypeList.Count; i++) {
			contextTypeNames[i] = contextTypeList[i].Name;
		}
	}

	public override void Render() {
		if (targetItem == null) return;
	    SerializedPropertyX property = rootProperty.FindProperty("contextType");
	    Type contextType = property.GetValue<Type>();

        if (contextType == null) {
			contextType = typeof(Context);
		}
		//shown = EditorGUILayout.Foldout(shown, "Context");
		//action : context factory
        int idx = contextTypeList.IndexOf(contextType);
        if (idx == -1) {
            idx = 0;
        }
        int newIdx = EditorGUILayout.Popup("Context Type", idx, contextTypeNames);
		if(newIdx != idx) {
			//todo pop up revalidate dialog
			//todo remove components / requirements where T does not match context type selected
			property.Value = contextTypeList[newIdx];
		}
	}
}