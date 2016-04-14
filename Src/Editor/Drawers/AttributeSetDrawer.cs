using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using System;

namespace AbilitySystem {

    [CustomPropertyDrawer(typeof(AbilityAttributeSet), true)]
    public class AttributeSetDrawer : ReorderableDrawer {
        private string[] attributeFormulaOptions;

        public override string LabelName {
            get { return "Attributes"; }
        }

        public override string ListPropertyName {
            get { return "attrs"; }
        }

        protected override void DrawElement(Rect baseRect, int index, bool isActive, bool isFocused) {
            EditorRect rect = new EditorRect(baseRect);
            var prop = new ImprovedSerializedProperty(property.GetArrayElementAtIndex(index));

            var idRect = rect.Width(120f);
            prop.Set("id", EditorGUI.TextField(idRect, prop.Get<string>("id")));

            var methodPointer = prop.Property("serializedMethodPointer");
            string[] options = GetOptions(property);

            int currentIndex = DrawerUtil.GetMatchingIndex(methodPointer.Get<string>("signature"), options);


            int idx = EditorGUI.Popup(rect.WidthMinus(50), currentIndex, options);
            if (currentIndex != idx) {
                if (idx == 0) {
                    methodPointer.Set("signature", "");
                }
                else {
                    methodPointer.Set("signature", attributeFormulaOptions[idx].ToString());
                }
            }

            prop.Set("baseValue", EditorGUI.FloatField(rect, prop.Get<float>("baseValue")));
        }

        protected override void DrawHeader(Rect rect) {
            var editorRect = new EditorRect(rect);
            editorRect.Width(12f);
            EditorGUI.LabelField(editorRect.Width(120), "Attribute");
            EditorGUI.LabelField(editorRect.WidthMinus(60), "Formula");
            EditorGUI.LabelField(editorRect.currentRect, "Base");
        }

        protected override void OnItemAdded(ReorderableList listArg) {
            var newElement = AddItemToList(listArg);
            newElement.FindPropertyRelative("id").stringValue = "";
            newElement.FindPropertyRelative("baseValue").floatValue = 0f;
            newElement.FindPropertyRelative("serializedMethodPointer").FindPropertyRelative("signature").stringValue = "";
        }

        public override void OnGUI(Rect position, SerializedProperty inputProperty, GUIContent label) {
            position = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            expanded = EditorGUI.Foldout(position, expanded, "Attributes");
            if (expanded) {
                position = new Rect(position) { y = position.y + EditorGUIUtility.singleLineHeight };

                property = inputProperty.FindPropertyRelative("attrs");
                if (property.arraySize > 0) {
                    GetOptions(property.GetArrayElementAtIndex(0));
                }
                var list = GetList(property);
                var height = 0f;
                for (var i = 0; i < property.arraySize; i++) {
                    height = Mathf.Max(height, EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i)));
                }

                list.elementHeight = height;
                list.DoList(position);
            }
        }

        private string[] GetOptions(SerializedProperty inputProperty) {
            if (attributeFormulaOptions != null) return attributeFormulaOptions;
            attributeFormulaOptions = DrawerUtil.GetFloatFormulaOptions<FormulaAttribute>(typeof(Ability));
            return attributeFormulaOptions;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!expanded) return EditorGUIUtility.singleLineHeight;
            return GetList(property.FindPropertyRelative("attrs")).GetHeight() + EditorGUIUtility.singleLineHeight;
        }

    }


}

