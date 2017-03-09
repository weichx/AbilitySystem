using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[PropertyDrawerFor(typeof(AbstractMethodPointer))]
public class MethodPointerDrawerX : PropertyDrawerX {

    protected List<MethodPointer> pointableMethods;

    public override void OnGUI(SerializedPropertyX source, GUIContent label) {
        Type sourceType = source.type;
        if (sourceType.IsGenericType) {
            Type[] genericTypes = sourceType.GetGenericArguments();
            Type[] args = new Type[genericTypes.Length - 1];
            for (int i = 0; i < (genericTypes.Length - 1); i++) {
                args[i] = genericTypes[i];
            }
            pointableMethods = Reflector.FindMethodPointersWithAttribute(typeof(Pointable), genericTypes[genericTypes.Length - 1], args);
        }
        else {
            pointableMethods = Reflector.FindMethodPointersWithAttribute(typeof(Pointable), typeof(void), Type.EmptyTypes);
        }
        if (source.Value == null) {
            source.Value = Activator.CreateInstance(source.type);
        }
        var displayList = new GUIContent[pointableMethods.Count + 1];
        displayList[0] = new GUIContent("-- None --");
        for (int i = 1; i < displayList.Length; i++) {
            displayList[i] = new GUIContent(pointableMethods[i - 1].signature);
        }
        SerializedPropertyX signature = source.FindProperty("signature");
        if (signature.Value == null) signature.Value = displayList[0].text;//
        int idx = Array.FindIndex(displayList, content => {
            return content.text == signature.Value as string;
        }); 
        if (idx == -1) idx = 0;
        int newIdx = EditorGUILayout.Popup(label, idx, displayList);
        signature.Value = displayList[newIdx].text;
    }
}

