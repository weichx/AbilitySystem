using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(MethodPointer))]
[CustomPropertyDrawer(typeof(AbstractMethodPointer))]
public class MethodPointerDrawer : PropertyDrawer {

    protected GUIContent[] content;
    protected List<MethodPointer> pointableMethods;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        SerializedProperty sigProp = property.FindPropertyRelative("signature");
        int idx = GetIndex(sigProp.stringValue);
        int newIdx = EditorGUI.Popup(position, label, idx, GetPointableMethods());
        if (newIdx != -1 && idx != newIdx) {
            sigProp.stringValue = pointableMethods[newIdx].signature;
        }
    }

    public int GetIndex(string signature) {
        GetPointableMethods();
        for(int i = 0; i < pointableMethods.Count; i++) {
            if(pointableMethods[i].signature == signature) {
                return i;
            }
        }
        return -1;
    }

    public GUIContent[] GetPointableMethods() {
        if (content != null) {
            return content;
        }
        pointableMethods = Reflector.FindMethodPointersWithAttribute(GetAttrType(), GetReturnType(), GetSignature());
        var retn = new List<GUIContent>(pointableMethods.Count);
        for (int i = 0; i < pointableMethods.Count; i++) {
            retn.Add(new GUIContent(pointableMethods[i].ShortSignature));
        }
        retn.TrimExcess();
        content = retn.ToArray();
        return content;
    }

    protected virtual Type GetAttrType() {
        return typeof(Pointable);
    }

    protected virtual Type GetReturnType() {
        return typeof(float);
    }

    protected virtual Type[] GetSignature() {
        return null;
    }
}

