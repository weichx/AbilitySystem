using System;
using UnityEditor;
using UnityEngine;

public class CastModeDrawable : IRectDrawable {

    protected SerializedProperty castMode;
    protected SerializedProperty ignoreGCD;

    public CastModeDrawable(SerializedProperty castMode, SerializedProperty ignoreGCD) {
        this.castMode = castMode;
        this.ignoreGCD = ignoreGCD;
    }

    public void Render(Rect rect) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(castMode, false);
        EditorGUILayout.PropertyField(ignoreGCD, false);
        EditorGUILayout.EndHorizontal();
    }

    public float GetHeight() {
        return Page.LineHeight;
    }

}