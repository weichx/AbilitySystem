using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public abstract class ReorderableDrawer : PropertyDrawer {

    protected bool expanded = true;
    protected ReorderableList reorderableList;
    protected SerializedProperty property;

    public abstract string LabelName { get; }
    public abstract string ListPropertyName { get; }

    public override void OnGUI(Rect position, SerializedProperty inputProperty, GUIContent label) {
        position = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
        expanded = EditorGUI.Foldout(position, expanded, LabelName);
        if (expanded) {
            position = new Rect(position) { y = position.y + EditorGUIUtility.singleLineHeight };

            property = inputProperty.FindPropertyRelative(ListPropertyName);
            var list = GetList(property);
            var height = 0f;
            for (var i = 0; i < property.arraySize; i++) {
                height = Mathf.Max(height, EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i)));
            }
            list.elementHeight = height;
            list.DoList(position);
        }
    }

    protected ReorderableList GetList(SerializedProperty property) {
        if (reorderableList != null) return reorderableList;
        reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true);
        reorderableList.drawHeaderCallback += DrawHeader;
        reorderableList.drawElementCallback += DrawElement;
        reorderableList.onAddCallback += OnItemAdded;
        return reorderableList;
    }

    protected virtual void OnDisable() {
        if (reorderableList == null) return;
        reorderableList.drawElementCallback -= DrawElement;
        reorderableList.drawHeaderCallback -= DrawHeader;
        reorderableList.onAddCallback += OnItemAdded;
        reorderableList = null;
    }

    protected abstract void DrawElement(Rect baseRect, int index, bool isActive, bool isFocused);
    protected abstract void DrawHeader(Rect rect);
    protected abstract void OnItemAdded(ReorderableList list);

    protected SerializedProperty AddItemToList(ReorderableList listArg) {
        int index = listArg.serializedProperty.arraySize;
        listArg.serializedProperty.arraySize++;
        listArg.index = index;
        return property.GetArrayElementAtIndex(index);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (!expanded) return EditorGUIUtility.singleLineHeight;
        return GetList(property.FindPropertyRelative(ListPropertyName)).GetHeight() + EditorGUIUtility.singleLineHeight;
    }
}