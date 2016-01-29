using UnityEditor;
using UnityEngine;
using AbilitySystem;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;

namespace AbilitySystem.Editor {
    
    [CustomPropertyDrawer(typeof(AttributeSet))]
    public class AttributeSetDrawer : PropertyDrawer {

        private string[] attributeFormulaOptions;
        private List<MethodPointer> pointerList;
        private ReorderableList reorderableList;
        private SerializedProperty property;

        private ReorderableList GetList(SerializedProperty property) {
            if (reorderableList != null) return reorderableList;
            reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true);
            reorderableList.drawHeaderCallback += DrawHeader;
            reorderableList.drawElementCallback += DrawElement;
            return reorderableList;
        }

        private void OnDisable() {
            if (reorderableList == null) return;
            reorderableList.drawElementCallback -= DrawElement;
            reorderableList.drawHeaderCallback -= DrawHeader;
            reorderableList = null;
        }

        private void DrawElement(Rect baseRect, int index, bool isActive, bool isFocused) {
            EditorRect rect = new EditorRect(baseRect);
            var prop = new ImprovedSerializedProperty(property.GetArrayElementAtIndex(index));

            var idRect = rect.Width(100f);
            prop.Set("id", EditorGUI.TextField(idRect, prop.Get<string>("id")));
            
            var formulaMetadata = prop.Property("formulaMetadata");
            //string methodName = formulaMetadata.Get<string>("method");
            //string typeName = formulaMetadata.Get<string>("type");
            //string retnTypeName = formulaMetadata.Get<string>("retnType");
            //string[] parameters = formulaMetadata.GetArray<string>("parameterTypes");

            int currentIndex = GetMatchingIndex(formulaMetadata.Get<string>("signature"));

            int idx = EditorGUI.Popup(rect.WidthMinus(50), currentIndex, attributeFormulaOptions);
            if (currentIndex != idx) {
                if(idx == 0) {
                    formulaMetadata.Set("signature", "");
                } else {
                    formulaMetadata.Set("signature", pointerList[idx - 1].ToString());
                }
            }

            prop.Set("baseValue", EditorGUI.FloatField(rect, prop.Get<float>("baseValue")));
        }

        protected virtual void DrawHeader(Rect rect) {
            EditorGUI.LabelField(rect, "Header");
        }

        public override void OnGUI(Rect position, SerializedProperty inputProperty, GUIContent label) {
            property = inputProperty.FindPropertyRelative("attrs");
            var attributeFormulaOptions = GetOptions();
            var list = GetList(property);
            var height = 0f;
            for (var i = 0; i < property.arraySize; i++) {
                height = Mathf.Max(height, EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i)));
            }
            list.elementHeight = height;
            list.DoList(position);
        }

        private int GetMatchingIndex(string signature) {
            if (signature == null) return 0;
            for (int i = 1; i < attributeFormulaOptions.Length; i++) {
                if (signature == attributeFormulaOptions[i]) return i;
            }
            return 0;
        }

        private string[] GetOptions() {
            if (attributeFormulaOptions != null) return attributeFormulaOptions;
            pointerList = Reflector.FindMethodPointersWithAttribute<AbilityAttributeFormula>(typeof(float), typeof(Entity), typeof(float));
            var formattedList = pointerList.Select((ptr) => {
                return ptr.ToString();
            }).ToList();
            formattedList.Insert(0, "-- None --");
            attributeFormulaOptions = formattedList.ToArray();
            return attributeFormulaOptions;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return GetList(property.FindPropertyRelative("attrs")).GetHeight();
        }
        
    }


}

