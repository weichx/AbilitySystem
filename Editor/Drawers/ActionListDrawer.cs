using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using System;

namespace AbilitySystem {

    //[CustomPropertyDrawer(typeof(ActionList), true)]
    public class ActionListDrawer : ReorderableDrawer {
        private string[] abilityActionTypeNames;
        private List<Type> abilityAcitonTypes;

        public override string LabelName {
            get { return "Actions"; }
        }

        public override string ListPropertyName {
            get { return "actions"; }
        }

        public override void OnGUI(Rect position, SerializedProperty inputProperty, GUIContent label) {
            position = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            expanded = EditorGUI.Foldout(position, expanded, LabelName);
            if (expanded) {
                position = new Rect(position) { y = position.y + EditorGUIUtility.singleLineHeight };

                property = inputProperty.FindPropertyRelative("actions");
                var list = GetList(property);
                var height = 0f;
                for (var i = 0; i < property.arraySize; i++) {
                    height = Mathf.Max(height, EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i)));
                }
                list.elementHeight = height;
                list.DoList(position);
            }
        }

        protected override void DrawElement(Rect baseRect, int index, bool isActive, bool isFocused) {
            EditorRect rect = new EditorRect(baseRect);
            var prop = new ImprovedSerializedProperty(property.GetArrayElementAtIndex(index));
            GetOptions(property);
            //var nameRect = rect.Width(120f);
            //Debug.Log(property.GetArrayElementAtIndex(index).FindPropertyRelative("name2"));
            //prop.Set("name2", EditorGUI.TextField(nameRect, prop.Get<string>("name2")));
            //string subclassName = (DrawerUtil.GetTarget(property) as Ability).actionList.actions[index].GetType().Name;
            //int idx = 0;
            //if (subclassName != "AbilityAction") {
            //    var x = abilityAcitonTypes.Find((type) => type.Name == subclassName);
            //    idx = abilityAcitonTypes.IndexOf(x) + 1;
            //}
            ////todo probably need an array of scriptable object subclasses
            //int newIdx = EditorGUI.Popup(rect, idx, abilityActionTypeNames);
            //if (idx != newIdx) {
            //    Debug.Log("NEW");
            //    property.InsertArrayElementAtIndex(index);
            //    var newElement = property.GetArrayElementAtIndex(index);
            //    //newElement.
            //}
        }

        protected override void DrawHeader(Rect rect) {
            var editorRect = new EditorRect(rect);
            editorRect.Width(12f);
            EditorGUI.LabelField(editorRect.Width(120), "Name");
            EditorGUI.LabelField(editorRect, "Type");
        }

        protected override void OnItemAdded(ReorderableList listArg) {
            var newElement = AddItemToList(listArg);
        }

        private void GetOptions(SerializedProperty inputProperty) {
            if (abilityAcitonTypes == null) {
                //abilityAcitonTypes = Reflector.FindSubClasses<AbilityAction>();
                //var names = abilityAcitonTypes.Select((t) => t.Name).ToList();// as List<string>;
                //names.Insert(0, "None");
                //abilityActionTypeNames = names.ToArray();
            }
        }
    }
}

