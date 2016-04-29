using UnityEditor;
using UnityEngine;

public class ComponentDrawable : IRectDrawable {

    protected SerializedProperty attr;
    protected bool isFolded;
    private GUIContent content;

    public ComponentDrawable(SerializedProperty attr) {
        this.attr = attr;
        content = new GUIContent(attr.type);
    }

    public void Render(Rect rect) {

        EditorGUILayout.PropertyField(attr, content, true);
    }

    public float GetHeight() {
        return EditorGUI.GetPropertyHeight(attr, content, true);
    }

}