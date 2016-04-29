using System;
using UnityEditor;
using UnityEngine;

public class FloatAttributeDrawable : IRectDrawable {

    protected SerializedProperty attr;

    public FloatAttributeDrawable(SerializedProperty attr) {
        this.attr = attr;
    }

    public void Render(Rect rect) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(attr, false);
        EditorGUILayout.EndHorizontal();
    }

    public float GetHeight() {
        return Page.LineHeight;
    }

}