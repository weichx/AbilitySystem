using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class TypePopup {

    public List<Type> typeList;
    public string[] typeNames;

    public TypePopup(Type inputType, bool useNullElement, Func<Type, bool> validator = null) {
        typeList = Reflector.FindSubClasses(inputType, true);
        if (validator == null) validator = NoOpValidator;
        typeList = typeList.FindAll((type) => {
            return !type.IsAbstract && !type.IsGenericTypeDefinition && validator(type);
        });
        if (useNullElement) {
            typeList.Insert(0, null);
        }
        typeNames = new string[typeList.Count];
        for (int i = 0; i < typeList.Count; i++) {
            if (typeList[i] != null) {
                typeNames[i] = typeList[i].Name;
            }
            else {
                typeNames[i] = " -- None --";
            }
        }
    }

    public bool DrawPopup(string label, Type currentType, out Type outType) {
        int idx = typeList.IndexOf(currentType);
        if (idx == -1) idx = 0;
        int newIdx = EditorGUILayout.Popup(label, idx, typeNames);
        outType = typeList[newIdx];
        return idx != newIdx;
    }

    private bool NoOpValidator(Type type) {
        return true;
    }

}