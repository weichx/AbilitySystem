using System;
using UnityEditor;
using UnityEngine;

public class ChargesDrawable : IRectDrawable {

    protected SerializedProperty attr;

    public ChargesDrawable(SerializedProperty attr) {
        this.attr = attr;
    }

    public void Render(Rect rect) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Charges");
        if(GUILayout.Button("+", GUILayout.Width(25f))) {
            attr.arraySize++;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        for (int i = 0; i < attr.arraySize; i++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(attr.GetArrayElementAtIndex(i).FindPropertyRelative("cooldown"), new GUIContent("Charge " + i));
            GUI.enabled = attr.arraySize > 1;
            if (GUILayout.Button("-", GUILayout.Width(25f), GUILayout.Height(15f))) {
                attr.DeleteArrayElementAtIndex(i);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
    }

    public float GetHeight() {
        return Page.LineHeight * (attr.arraySize + 1);
    }

}