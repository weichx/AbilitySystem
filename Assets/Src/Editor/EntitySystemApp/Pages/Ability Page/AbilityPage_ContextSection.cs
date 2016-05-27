using System;
using UnityEditor;
using UnityEngine;
using Intelligence;
using Context = Intelligence.Context;
using System.Collections.Generic;

public class AbilityPage_ContextSection : AbilityPage_SectionBase {

	private bool shown;
	private List<Type> contextTypeList;
	private string[] contextTypeNames;

	public AbilityPage_ContextSection() {
		shown = true;
		contextTypeList = Reflector.FindSubClasses<Intelligence.Context>(true);
		contextTypeNames = new string[contextTypeList.Count];
		for(int i = 0; i < contextTypeList.Count; i++) {
			contextTypeNames[i] = contextTypeList[i].Name;
		}
	}

	public override void Render() {
		if (serialRoot == null) return;
		if(targetItem.InstanceRef.contextType == null) {
			targetItem.InstanceRef.contextType = typeof(Intelligence.Context);
		}
		//shown = EditorGUILayout.Foldout(shown, "Context");
		//action : context factory
		int idx = GetContextTypeIndex();
		int newIdx = EditorGUILayout.Popup("Context Type", idx, contextTypeNames);
		if(newIdx != idx) {
			//todo pop up revalidate dialog
			//todo remove components / requirements where T does not match context type selected
			targetItem.InstanceRef.contextType = contextTypeList[newIdx];
			//type might not be serializable through unity, may store type name ref instead
			targetItem.SerializedObject.Update();
		}
	}

	private int GetContextTypeIndex() {
		Type type = targetItem.InstanceRef.contextType;
		int retn = contextTypeList.IndexOf(type);
		if(retn == -1) {
			retn = 0;
		}
		return retn;
	}

}