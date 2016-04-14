using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

namespace AbilitySystem {

    [CustomPropertyDrawer(typeof(TagCollection))]
    public class TagCollectionDrawer : PropertyDrawer {
        private bool expanded = true;//todo use reorderable drawer
        private ReorderableList reorderableList;
        private SerializedProperty property;

        private ReorderableList GetList(SerializedProperty property) {
            if (reorderableList != null) return reorderableList;
            reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true);
            reorderableList.drawElementCallback += DrawElement;
            reorderableList.drawHeaderCallback += DrawHeader;
            reorderableList.onAddCallback += AddTag;
            return reorderableList;
        }

        public override void OnGUI(Rect position, SerializedProperty inputProperty, GUIContent label) {
            position = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            expanded = EditorGUI.Foldout(position, expanded, label);
            if(expanded) {
                position = new Rect(position) { y = position.y + EditorGUIUtility.singleLineHeight };

                property = inputProperty.FindPropertyRelative("tags");
                var list = GetList(property);
                var height = 0f;
                for (var i = 0; i < property.arraySize; i++) {
                    height = Mathf.Max(height, EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i)));
                }
                list.elementHeight = height;
                list.DoList(position);
            }
        }

        protected virtual void AddTag(ReorderableList list) {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var newElement = property.GetArrayElementAtIndex(index);
            newElement.FindPropertyRelative("name").stringValue = "New Tag";
        }

        protected virtual void DrawHeader(Rect rect) {
            EditorGUI.LabelField(rect, new GUIContent(""));
        }

        private void OnDisable() {
            if (reorderableList == null) return;
            reorderableList.drawElementCallback -= DrawElement;
            reorderableList.drawHeaderCallback -= DrawHeader;
            reorderableList.onAddCallback -= AddTag;
            reorderableList = null;
        }

        private void DrawElement(Rect baseRect, int index, bool isActive, bool isFocused) {
            var prop = new ImprovedSerializedProperty(property.GetArrayElementAtIndex(index));
            prop.Set("name", EditorGUI.TextField(baseRect, prop.Get<string>("name")));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!expanded) return EditorGUIUtility.singleLineHeight;
            return GetList(property.FindPropertyRelative("tags")).GetHeight() + EditorGUIUtility.singleLineHeight;
        }
    }
}